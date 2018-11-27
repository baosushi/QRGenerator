using GSuiteChromeExtension.Common.Services;
using GSuiteChromeExtension.Common.ViewModels;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Parsing;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;

namespace GSuiteChromeExtension.Pdf.Api.Models
{
    public class PdfManipulator
    {

        public const string PdfContentType = "application/pdf";

        private static readonly string UploadPath = HostingEnvironment.MapPath(ConfigurationManager.AppSettings["PdfUploadFilePath"]);
        private static readonly string WkhtmltopdfExePath = ConfigurationManager.AppSettings["WkhtmltopdfExePath"];

        private static readonly string PdfManipulatorPath = HostingEnvironment.MapPath("~/Libs/PDFManipulator.exe");
        private static readonly string GhostScriptPath = ConfigurationManager.AppSettings["GhostScriptPath"];

        private const string OutputMergeFileName = "merged.pdf";
        private const string InputCompressFileName = "compress_input.pdf";
        private const string OutputCompressFileName = "compressed.pdf";

        public string Id { get; set; }
        public string Folder { get; private set; }
        public string OriginalFileName { get; private set; }

        public List<string> InputFiles { get; private set; }
        public string InputFile { get; private set; }

        public string ResultFolder
        {
            get
            {
                return Path.Combine(this.Folder, "Result");
            }
        }

        public FileInfo OutputFile
        {
            get
            {
                return new DirectoryInfo(this.ResultFolder)
                    .GetFiles()
                    .FirstOrDefault();
            }
        }

        public string OutputMergedFile
        {
            get
            {
                return Path.Combine(this.ResultFolder, OutputMergeFileName);
            }
        }
        public string OutputCompressedFile
        {
            get
            {
                return Path.Combine(this.ResultFolder, OutputCompressFileName);
            }
        }

        public bool ExistMerged
        {
            get
            {
                return File.Exists(this.OutputMergedFile);
            }
        }

        public bool ExistCompressed
        {
            get
            {
                return File.Exists(this.OutputCompressedFile);
            }
        }

        public PdfManipulator() : this(null, false) { }

        public PdfManipulator(string id, bool throwIfNotExist)
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

            this.InputFiles = Directory.GetFiles(this.Folder).ToList();
            this.InputFile = this.InputFiles.FirstOrDefault();

            if (this.InputFile != null)
            {
                this.OriginalFileName = Path.GetFileName(this.InputFile);
            }
        }

        public async Task<int> SaveFilesAsync(IEnumerable<HttpContent> files)
        {
            var result = 0;

            this.InputFiles = new List<string>();
            foreach (var file in files)
            {
                var extension = Path.GetFileName(file.GetFileName());

                var fileName = $"{result}{extension}";
                var filePath = Path.Combine(this.Folder, fileName);

                File.WriteAllBytes(filePath, await file.ReadAsByteArrayAsync());

                this.InputFiles.Add(filePath);
                result++;
            }

            return result;
        }

        public async Task<int> SaveFilesAsync(DefaultGoogleMultipleFilesRequest filesRequest)
        {
            var result = 0;

            this.InputFiles = new List<string>();

            var googleDriveService = new GoogleDriveService();

            foreach (var file in filesRequest.Files)
            {
                var extension = Path.GetExtension(file.Name);
                var fileName = $"{result}{extension}";
                var filePath = Path.Combine(this.Folder, fileName);

                await googleDriveService.DownloadFileAsync(file, filesRequest.OAuthToken, this.Folder, fileName);

                this.InputFiles.Add(filePath);
                result++;
            }

            return result;
        }

        public async Task SaveFileAsync(HttpContent file)
        {
            this.InputFile = Path.Combine(this.Folder, InputCompressFileName);
            this.OriginalFileName = file.GetFileName();
            File.WriteAllBytes(this.InputFile, await file.ReadAsByteArrayAsync());
        }

        public async Task PerformMergeWithGSAsync()
        {
            await Task.Run(() =>
            {
                var input = new StringBuilder();
                foreach (var file in this.InputFiles)
                {
                    input.AppendFormat(" \"{0}\"", file);
                }

                var processStartInfo = new ProcessStartInfo(
                    GhostScriptPath,
                    $"-sDEVICE=pdfwrite -dCompatibilityLevel=1.4 -dPDFSETTINGS=/printer -dNOPAUSE -dQUIET -dBATCH -sOutputFile=\"{this.OutputMergedFile}\" {input.ToString()}")
                {
                    CreateNoWindow = true,
                };


                var process = Process.Start(processStartInfo);
                if (!process.WaitForExit(60000))
                {
                    try
                    {
                        process.Kill();
                    }
                    catch (Exception) { }
                    throw new OperationCanceledException("There was a problem with the merging operation.");
                }
            });

            // Check for output file's existence
            if (!this.ExistMerged)
            {
                throw new OperationCanceledException("There was a problem with the merging operation.");
            }
        }

        public async Task PerformCompressWithGSAsync()
        {
            await Task.Run(() =>
            {
                var info = new ProcessStartInfo(GhostScriptPath,
                    $"-sDEVICE=pdfwrite -dCompatibilityLevel=1.4 -dPDFSETTINGS=/printer -dNOPAUSE -dQUIET -dBATCH -sOutputFile=\"{this.OutputCompressedFile}\" \"{this.InputFile}\"")
                {
                    CreateNoWindow = true,
                };

                var process = Process.Start(info);
                if (!process.WaitForExit(60000))
                {
                    try
                    {
                        process.Kill();
                    }
                    catch (Exception) { }
                    throw new OperationCanceledException("There was a problem with the compressing operation.");
                }
            });

            // Check for output file's existence
            if (!this.ExistCompressed)
            {
                throw new OperationCanceledException("There was a problem with the compressing operation.");
            }
        }

        public async Task AddPasswordAsync(string userPassword, string ownerPassword)
        {
            await Task.Run(() =>
            {
                var fileContent = File.ReadAllBytes(this.InputFile);

                using (var loadedDocument = new PdfLoadedDocument(fileContent))
                {
                    loadedDocument.Security.KeySize = Syncfusion.Pdf.Security.PdfEncryptionKeySize.Key256Bit;

                    if (!string.IsNullOrWhiteSpace(ownerPassword))
                    {
                        loadedDocument.Security.OwnerPassword = ownerPassword;
                    }

                    if (!string.IsNullOrWhiteSpace(userPassword))
                    {
                        loadedDocument.Security.UserPassword = userPassword;
                    }

                    var fileName = Path.GetFileNameWithoutExtension(this.OriginalFileName);
                    var outputFile = Path.Combine(this.ResultFolder, fileName + "_password.pdf");
                    loadedDocument.Save(outputFile);
                    loadedDocument.Close();
                }   
            });
        }

        public async Task RemovePasswordAsync(string ownerPassword)
        {
            await Task.Run(() =>
            {
                var fileContent = File.ReadAllBytes(this.InputFile);

                using (var loadedDocument = new PdfLoadedDocument(fileContent, ownerPassword))
                {
                    loadedDocument.Security.OwnerPassword = "";
                    loadedDocument.Security.UserPassword = "";

                    var fileName = Path.GetFileNameWithoutExtension(this.OriginalFileName);
                    var outputFile = Path.Combine(this.ResultFolder, fileName + "_nopassword.pdf");
                    loadedDocument.Save(outputFile);

                    loadedDocument.Close();
                }   
            });
        }

        public async Task SplitAsync()
        {
            await Task.Run(() =>
            {
                var fileContent = File.ReadAllBytes(this.InputFile);

                var loadedDocument = new PdfLoadedDocument(fileContent);

                var fileName = Path.GetFileNameWithoutExtension(this.OriginalFileName);
                var outputFile = Path.Combine(this.ResultFolder, fileName + "_split.zip");

                if (File.Exists(outputFile))
                {
                    File.Delete(outputFile);
                }

                using (var fileStream = new FileStream(outputFile, FileMode.Create))
                {
                    using (var zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Create))
                    {
                        for (int i = 0; i < loadedDocument.Pages.Count; i++)
                        {
                            var entry = zipArchive.CreateEntry($"Page_{(i + 1).ToString("0000")}.pdf", CompressionLevel.NoCompression);

                            var pdfDocument = new PdfDocument();
                            pdfDocument.ImportPage(loadedDocument, i);

                            using (var stream = entry.Open())
                            {
                                pdfDocument.Save(stream);
                            }
                        }
                    }
                }
            });
        }

        public async Task RotateAsync(int degree, bool flipX, bool flipY, IEnumerable<int> pages)
        {
            await Task.Run(() =>
            {
                var fileContent = File.ReadAllBytes(this.InputFile);
                var loadedDocument = new PdfLoadedDocument(fileContent);

                var pdfDocument = new PdfDocument();

                for (int i = 0; i < loadedDocument.Pages.Count; i++)
                {
                    pdfDocument.ImportPage(loadedDocument, i);

                    if (pages == null || pages.Contains(i + 1))
                    {
                        var rotateAngle = PdfPageRotateAngle.RotateAngle90;

                        switch (degree)
                        {
                            case 0:
                                rotateAngle = PdfPageRotateAngle.RotateAngle0;
                                break;
                            case 90:
                                rotateAngle = PdfPageRotateAngle.RotateAngle90;
                                break;
                            case 180:
                                rotateAngle = PdfPageRotateAngle.RotateAngle180;
                                break;
                            case 270:
                                rotateAngle = PdfPageRotateAngle.RotateAngle270;
                                break;
                        }

                        pdfDocument.Pages[i].Section.PageSettings.Rotate = rotateAngle;
                    }
                }

                var fileName = Path.GetFileNameWithoutExtension(this.OriginalFileName);
                var outputFile = Path.Combine(this.ResultFolder, fileName + "_rotated.pdf");
                pdfDocument.Save(outputFile);
            });
        }

        public async Task RenderWebAsync(string url)
        {
            if (url.IndexOf("//") == -1)
            {
                url = "http://" + url;
            }

            var hostName = new Uri(url).Host.ToLower();

            if (hostName == "localhost")
            {
                throw new OperationCanceledException("Invalid Hostname");
            }

            var outputFileName = hostName + ".pdf";
            var fullFilePath = Path.Combine(this.ResultFolder, outputFileName);

            await Task.Run(() =>
            {
                var processStartInfo = new ProcessStartInfo(
                    WkhtmltopdfExePath,
                    $"{url} \"{fullFilePath}\"")
                {
                    CreateNoWindow = true,
                };

                var process = Process.Start(processStartInfo);
                if (!process.WaitForExit(60000))
                {
                    try
                    {
                        process.Kill();
                    }
                    catch (Exception)
                    {
                    }

                    throw new OperationCanceledException("There was a problem with the rendering operation.");
                }
            });

            // Check for output file's existence
            if (!File.Exists(fullFilePath))
            {
                throw new OperationCanceledException("There was a problem with the rendering operation.");
            }
        }

    }
}