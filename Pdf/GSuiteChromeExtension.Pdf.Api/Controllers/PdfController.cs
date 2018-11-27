using GSuiteChromeExtension.Common.ViewModels;
using GSuiteChromeExtension.Pdf.Api.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;

namespace GSuiteChromeExtension.Pdf.Api.Controllers
{

    [CorsPolicy]
    [RoutePrefix("api/pdf")]
    public class PdfController : ApiController
    {

        [HttpPost, Route("upload")]
        public async Task<IHttpActionResult> Upload()
        {
            var multipartReader = new MultipartReader();
            await this.Request.Content.ReadAsMultipartAsync(multipartReader);

            List<HttpContent> files;
            if (!multipartReader.Values.TryGetValue(nameof(files), out files) || files.Count == 0)
            {
                return this.BadRequest("No file");
            }

            var pdfManipulator = new PdfManipulator();
            await pdfManipulator.SaveFilesAsync(files);

            return this.Json(new
            {
                id = pdfManipulator.Id,
            });
        }

        [HttpPost, Route("upload-drive")]
        public async Task<IHttpActionResult> UploadDrive([FromBody] DefaultGoogleMultipleFilesRequest request)
        {
            if (request == null || request.Files == null || request.Files.Count() == 0 ||
                string.IsNullOrEmpty(request.OAuthToken))
            {
                return this.BadRequest();
            }

            var pdfManipulator = new PdfManipulator();
            await pdfManipulator.SaveFilesAsync(request);

            return this.Json(new
            {
                id = pdfManipulator.Id,
            });
        }

        [HttpGet, Route("convert")]
        public async Task<IHttpActionResult> Convert(string id, string output)
        {
            PdfConvert converter;
            try
            {
                converter = new PdfConvert(id, true);
            }
            catch (Exception)
            {
                return this.NotFound();
            }

            var inputFile = converter.GetInputFilePath(true);
            switch (output)
            {
                case "doc":
                case "word":
                    await converter.ConvertToWordAsync(inputFile);
                    break;
                case "tiff":
                case "png":
                case "jpg":
                case "jpeg":
                    await converter.ConvertToImageAsync(inputFile, output);
                    break;
                case "pdf":
                    await converter.ConvertToPdfAsync(inputFile);
                    break;
                default:
                    return this.BadRequest("Invalid Output Type");
            }

            var outputFile = Path.GetFileName(converter.ConvertedFile);
            return this.Json(new
            {
                id = converter.Id,
                outputFile = outputFile,
            });
        }

        [HttpGet, Route("manipulate")]
        public async Task<IHttpActionResult> Manipulate(string id, string operation)
        {
            PdfManipulator pdfManipulator;
            try
            {
                pdfManipulator = new PdfManipulator(id, true);
            }
            catch (Exception)
            {
                return this.NotFound();
            }

            var queryString = this.Request.RequestUri.ParseQueryString();
            switch (operation)
            {
                case "compress":
                    await pdfManipulator.PerformCompressWithGSAsync();
                    break;
                case "combine":
                    await pdfManipulator.PerformMergeWithGSAsync();
                    break;
                case "add-password":
                    {
                        string userPassword, ownerPassword;
                        userPassword = queryString[nameof(userPassword)]?.ToString();
                        ownerPassword = queryString[nameof(ownerPassword)]?.ToString();

                        await pdfManipulator.AddPasswordAsync(userPassword, ownerPassword);
                    }

                    break;
                case "remove-password":
                    {
                        string ownerPassword;
                        ownerPassword = queryString[nameof(ownerPassword)]?.ToString();

                        await pdfManipulator.RemovePasswordAsync(ownerPassword);
                    }

                    break;
                case "split":
                    await pdfManipulator.SplitAsync();
                    break;
                case "rotate":
                    {
                        string degree, pages;
                        bool flipX, flipY;

                        degree = queryString[nameof(degree)]?.ToString();
                        if (degree != "0" && degree != "90" && degree != "180" && degree != "270")
                        {
                            return this.BadRequest("No degree.");
                        };

                        flipX = queryString[nameof(flipX)]?.ToString() == "true";
                        flipY = queryString[nameof(flipY)]?.ToString() == "true";

                        pages = queryString[nameof(pages)]?.ToString();
                        var pagesList = this.ParsePages(pages);

                        await pdfManipulator.RotateAsync(int.Parse(degree), flipX, flipY, pagesList);
                    }

                    break;
                default:
                    return this.BadRequest("Invalid operation");
            }

            var inputFile = new FileInfo(pdfManipulator.InputFile);
            var outputFile = pdfManipulator.OutputFile;

            return this.Json(new
            {
                id = pdfManipulator.Id,
                inputFileSize = inputFile.Length,
                outputFile = outputFile?.Name,
                outputFileSize = outputFile?.Length,
            });
        }

        [HttpGet, Route("download")]
        public HttpResponseMessage Download(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No id");
            }

            var converter = new PdfConvert(id, true);
            if (converter.ConvertedFile == null)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, "Id not found");
            }

            var stream = new FileStream(converter.ConvertedFile, FileMode.Open, FileAccess.Read);
            var result = new HttpResponseMessage()
            {
                Content = new StreamContent(stream),
            };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            var fileName = Path.GetFileName(converter.ConvertedFile);
            result.Content.Headers.Add("Content-Disposition", "attachment; filename=\"" + fileName + "\"");

            return result;
        }

        [HttpGet, Route("render")]
        public async Task<IHttpActionResult> Render(string url, string googleRecaptcha)
        {
            if (!await this.ValidateGoogleRecaptchaAsync(googleRecaptcha))
            {
                return this.BadRequest("Invalid Recaptcha");
            }

            var pdfManipulator = new PdfManipulator();
            var outputFile = pdfManipulator.OutputFile;

            await pdfManipulator.RenderWebAsync(url);
            return this.Json(new
            {
                id = pdfManipulator.Id,
                outputFile = outputFile?.Name,
            });
        }

        private static readonly char[] PageSeparator = { ',' };
        [NonAction]
        private List<int> ParsePages(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

            try
            {
                var parts = input.Split(PageSeparator, StringSplitOptions.RemoveEmptyEntries);
                var result = new List<int>();
                foreach (var part in parts)
                {
                    if (part.Contains("-"))
                    {
                        var subParts = part.Split('-');

                        var from = int.Parse(subParts[0].Trim());
                        var to = int.Parse(subParts[1].Trim());

                        for (int i = from; i <= to; i++)
                        {
                            result.Add(i);
                        }
                    }
                    else
                    {
                        result.Add(int.Parse(part.Trim()));
                    }
                }

                return result;
            }
            catch (Exception)
            {
                return null;
            }

        }

        private static readonly RestClient RecaptchaClient = new RestClient("https://www.google.com/");
        [NonAction]
        private async Task<bool> ValidateGoogleRecaptchaAsync(string code)
        {
            var request = new RestRequest("recaptcha/api/siteverify", Method.POST);
            request.AddParameter("secret", "6LeR1jMUAAAAAHh59YbdCmOw73q3NY554UEqU3il");
            request.AddParameter("response", code);

            var response = await RecaptchaClient.ExecuteTaskAsync(request);
            if (response.ResponseStatus != ResponseStatus.Completed || response.StatusCode != HttpStatusCode.OK)
            {
                return false;
            }

            return JsonConvert.DeserializeObject<dynamic>(response.Content).success;
        }

    }

}
