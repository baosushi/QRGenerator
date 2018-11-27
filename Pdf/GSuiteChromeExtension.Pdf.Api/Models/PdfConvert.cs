using GSuiteChromeExtension.Common;
using SautinSoft;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocToPDFConverter;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;

namespace GSuiteChromeExtension.Pdf.Api.Models
{

    public class PdfConvert
    {

        private static readonly string PdfFocusLicenseKey = ConfigurationManager.AppSettings["PdfFocusLicenseKey"];
        private static readonly string UploadPath = HostingEnvironment.MapPath(ConfigurationManager.AppSettings["PdfUploadFilePath"]);

        public string Id { get; set; }
        public string Folder { get; private set; }

        public bool Exist
        {
            get
            {
                return Directory.Exists(this.Folder);
            }
        }

        public string InputFile
        {
            get
            {
                return Directory.GetFiles(this.Folder).FirstOrDefault();
            }
        }

        public string ResultFolder
        {
            get
            {
                return Path.Combine(this.Folder, "Result");
            }
        }

        public string ConvertedFile
        {
            get
            {
                return Directory.GetFiles(this.ResultFolder)
                    .FirstOrDefault();
            }
        }

        public PdfConvert() : this(null, false) { }

        public PdfConvert(string id, bool throwIfNotExist)
        {
            this.Id = id ?? Guid.NewGuid().ToString();

            this.Folder = Path.Combine(
                UploadPath,
                this.Id);

            if (throwIfNotExist && !Directory.Exists(this.Folder))
            {
                throw new IOException("The id does not exist.");
            }

            // Create the directory if needed
            Directory.CreateDirectory(this.Folder);
            Directory.CreateDirectory(this.ResultFolder);
        }

        public string GetInputFilePath(bool throwIfNotExist)
        {
            var inputFile = this.InputFile;

            if (throwIfNotExist && inputFile == null)
            {
                throw new IOException("No input file");
            }

            return inputFile;
        }

        public async Task ConvertToWordAsync(string inputFilePath)
        {
            await Task.Run(() =>
            {
                using (var fileStream = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read))
                {
                    var pdfFocus = CreatePdfFocus();
                    pdfFocus.OpenPdf(fileStream);

                    var inputFileName = Path.GetFileNameWithoutExtension(inputFilePath);
                    var convertedFileName = inputFileName + ".doc";

                    var outputFilePath = Path.Combine(this.ResultFolder, convertedFileName);

                    pdfFocus.ToWord(outputFilePath);
                }
            });
        }

        public async Task ConvertToImageAsync(string inputFilePath, string extension)
        {
            await Task.Run(() =>
            {
                using (var inputFileStream = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read))
                {
                    var pdfFocus = CreatePdfFocus();
                    pdfFocus.OpenPdf(inputFilePath);

                    var inputFileName = Path.GetFileNameWithoutExtension(inputFilePath);
                    var convertedFileName = CommonUtils.ChangeExtension(inputFilePath, "." + extension);
                    var filePath = Path.Combine(this.ResultFolder, convertedFileName);

                    switch (extension)
                    {
                        case "jpg":
                        case "jpeg":
                            pdfFocus.ImageOptions.ImageFormat = ImageFormat.Jpeg;
                            break;
                        case "png":
                            pdfFocus.ImageOptions.ImageFormat = ImageFormat.Png;
                            break;
                        case "tiff":
                            pdfFocus.ImageOptions.ImageFormat = ImageFormat.Tiff;
                            break;
                    }

                    if (extension == "tiff")
                    {
                        pdfFocus.ToMultipageTiff(filePath);
                    }
                    else if (pdfFocus.PageCount < 2)
                    {
                        pdfFocus.ToImage(filePath, 1);
                    }
                    else
                    {
                        var images = pdfFocus.ToImage();
                        convertedFileName = CommonUtils.ChangeExtension(inputFilePath, ".zip");
                        filePath = Path.Combine(this.ResultFolder, convertedFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create))
                            {
                                for (int i = 0; i < images.Count; i++)
                                {
                                    var image = images[i] as byte[];
                                    if (image == null)
                                    {
                                        continue;
                                    }

                                    var entry = archive.CreateEntry($"{inputFileName}-{i + 1}.{extension}", CompressionLevel.Fastest);

                                    using (var entryStream = entry.Open())
                                    {
                                        entryStream.Write(image, 0, image.Length);
                                    }
                                }
                            }
                        }
                    }
                }
            });
        }

        public async Task ConvertToPdfAsync(string inputFilePath)
        {
            await Task.Run(() =>
            {
                using (var fileStream = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read))
                {
                    var outputFileName = CommonUtils.ChangeExtension(inputFilePath, ".pdf");
                    var outputFilePath = Path.Combine(this.ResultFolder, outputFileName);

                    var wordDocument = new WordDocument(fileStream);
                    var converter = new DocToPDFConverter();
                    var pdfDocument = converter.ConvertToPDF(wordDocument);
                    pdfDocument.Save(outputFilePath);
                    pdfDocument.Close(true);
                    wordDocument.Close();
                }
            });
        }

        private static PdfFocus CreatePdfFocus()
        {
            return new PdfFocus()
            {
                Serial = PdfFocusLicenseKey,
            };
        }

    }

}