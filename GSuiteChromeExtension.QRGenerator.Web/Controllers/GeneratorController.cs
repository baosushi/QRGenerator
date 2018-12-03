using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Novacode;

namespace GSuiteChromeExtension.QRGenerator.Web.Controllers
{
    [Route("api/generator")]
    [ApiController]
    public class GeneratorController : ControllerBase
    {
        IHostingEnvironment env;
        public GeneratorController(IHostingEnvironment env)
        {
            this.env = env;
        }

        [HttpPost, Route("generate")]
        public async Task<IActionResult> Generate([FromBody]GenerateRequestViewModel request)
        {
            var doc = DocX.Create(Path.Combine(env.WebRootPath, "Upload", $"{request.File.Id}.docx"));

            if (request.IsFolder)
            {
                var url = $"https://www.googleapis.com/drive/v3/files?fields=files(id,name,webViewLink)&q='{request.File.Id}' in parents";
                GoogleFolder folder = await GetGoogleApiResponse<GoogleFolder>(url, request.Token);

                try
                {
                    if (request.WithDescription)
                    {
                        var table = doc.AddTable(folder.Files.Count * request.Repetition, 2);
                        table.Alignment = Alignment.center;
                        table.SetWidthsPercentage(new[] { 30f, 70f }, 595);
                        int count = 0;

                        foreach (var file in folder.Files)
                        {
                            byte[] imageData = null;
                            MemoryStream ms;

                            ms = null;

                            try
                            {
                                using (var wc = new WebClient())
                                {
                                    imageData = wc.DownloadData($"https://chart.googleapis.com/chart?chs=150x150&cht=qr&chl={file.WebViewLink}");
                                }
                                ms = new MemoryStream(imageData);
                            }
                            catch (Exception ex)
                            {
                                //forbidden, proxy issues, file not found (404) etc
                            }

                            var image = doc.AddImage(ms, "image/png");
                            var picture = image.CreatePicture();

                            for (int i = 0; i < request.Repetition; i++)
                            {
                                var row = table.Rows[count];
                                row.Cells[0].MarginLeft = 0;
                                var para1 = row.Cells[0].InsertParagraph();
                                para1.AppendPicture(picture);
                                para1.Alignment = Alignment.center;

                                row.Cells[1].MarginLeft = 0;
                                row.Cells[1].MarginTop = 0;
                                row.Cells[1].VerticalAlignment = VerticalAlignment.Center;
                                var para2 = row.Cells[1].InsertParagraph();
                                para2.InsertText(file.Name);
                                para2.Alignment = Alignment.center;
                                count++;
                            }
                        }

                        doc.InsertTable(table);
                    }
                    else
                    {
                        var table = doc.AddTable((int)Math.Ceiling(folder.Files.Count * request.Repetition / 4.0), 4);
                        table.Alignment = Alignment.center;
                        var row = table.Rows[0];
                        var rowCount = 0;
                        var count = 0;

                        foreach (var file in folder.Files)
                        {
                            byte[] imageData = null;
                            MemoryStream ms = null;

                            try
                            {
                                using (var wc = new WebClient())
                                {
                                    imageData = wc.DownloadData($"https://chart.googleapis.com/chart?chs=150x150&cht=qr&chl={file.WebViewLink}");
                                }
                                ms = new MemoryStream(imageData);
                            }
                            catch (Exception ex)
                            {
                                //forbidden, proxy issues, file not found (404) etc
                            }

                            var image = doc.AddImage(ms, "image/png");
                            var picture = image.CreatePicture();

                            for (var i = 0; i < request.Repetition; i++)
                            {
                                if (count >= 4)
                                {
                                    count = 0;
                                    row = table.Rows[++rowCount];
                                }

                                row.Cells[count].MarginLeft = 0;
                                var para1 = row.Cells[count].InsertParagraph();
                                para1.AppendPicture(picture);
                                para1.Alignment = Alignment.center;
                                var para2 = row.Cells[count].InsertParagraph();
                                para2.InsertText(file.Name);
                                para2.Alignment = Alignment.center;
                                count++;
                            }
                        }

                        doc.InsertTable(table);
                    }
                }
                catch (Exception e)
                {
                    return this.StatusCode(500, e.Message);
                }
            }
            else
            {
                var url = $"https://www.googleapis.com/drive/v3/files/{request.File.Id}?fields=id,name,webViewLink";
                ResponseItemViewModel item = await GetGoogleApiResponse<ResponseItemViewModel>(url, request.Token);

                try
                {
                    byte[] imageData = null;
                    MemoryStream ms = null;

                    try
                    {
                        using (var wc = new WebClient())
                        {
                            imageData = wc.DownloadData($"https://chart.googleapis.com/chart?chs=150x150&cht=qr&chl={item.WebViewLink}");
                        }
                        ms = new MemoryStream(imageData);
                    }
                    catch (Exception ex)
                    {
                        //forbidden, proxy issues, file not found (404) etc
                    }

                    var image = doc.AddImage(ms, "image/png");
                    var picture = image.CreatePicture();

                    if (request.WithDescription)
                    {
                        var table = doc.AddTable(request.Repetition, 2);
                        table.Alignment = Alignment.center;
                        table.SetWidthsPercentage(new[] { 30f, 70f }, 595);

                        foreach (var row in table.Rows)
                        {
                            row.Cells[0].MarginLeft = 0;
                            var para1 = row.Cells[0].InsertParagraph();
                            para1.AppendPicture(picture);
                            para1.Alignment = Alignment.center;

                            row.Cells[1].MarginLeft = 0;
                            row.Cells[1].MarginTop = 0;
                            row.Cells[1].VerticalAlignment = VerticalAlignment.Center;
                            var para2 = row.Cells[1].InsertParagraph();
                            para2.InsertText(item.Name);
                            para2.Alignment = Alignment.center;
                        }

                        doc.InsertTable(table);
                    }
                    else
                    {
                        var table = doc.AddTable((int)Math.Ceiling(request.Repetition / 4.0), 4);
                        table.Alignment = Alignment.center;
                        table.Design = TableDesign.TableGrid;
                        var count = 0;

                        for (int i = 0; i < table.Rows.Count && count < request.Repetition; i++)
                        {
                            for (int j = 0; j < 4 && count < request.Repetition; j++)
                            {
                                table.Rows[i].Cells[j].MarginLeft = 0;
                                var para1 = table.Rows[i].Cells[j].InsertParagraph();
                                para1.AppendPicture(picture);
                                para1.Alignment = Alignment.center;
                                var para2 = table.Rows[i].Cells[j].InsertParagraph();
                                para2.InsertText(item.Name, false);
                                para2.Alignment = Alignment.center;
                                count++;
                            }
                        }

                        doc.InsertTable(table);
                    }
                }
                catch (Exception e)
                {
                    return this.StatusCode(500, e.Message);
                }
            }

            var path = Path.Combine(env.WebRootPath, "Upload");
            Directory.CreateDirectory(path);

            doc.Save();

            return this.Ok(new
            {
                Path = "/Upload/" + Uri.EscapeDataString($"{request.File.Id}.docx"),
            });
        }

        private async Task<T> GetGoogleApiResponse<T>(string url, string token)
        {
            T item = default(T);
            var httpRequest = WebRequest.CreateHttp(url);
            httpRequest.Headers.Add("Authorization", "Bearer " + token);

            using (var response = (await httpRequest.GetResponseAsync()) as HttpWebResponse)
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new WebException($"Unexpected Status Code: {response.StatusCode} ({response.StatusDescription})");
                }

                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    item = JsonConvert.DeserializeObject<T>(reader.ReadToEnd());
                }
            }

            return item;
        }
    }

    public class ResponseItemViewModel : GoogleFile
    {
        [JsonProperty("webViewLink")]
        public string WebViewLink { get; set; }
    }

    public class GenerateRequestViewModel
    {
        public string Token { get; set; }
        public GoogleFile File { get; set; }
        public bool IsFolder { get; set; }
        public bool WithDescription { get; set; }
        public int Repetition { get; set; }
    }

    public class GoogleFile
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class GoogleFolder
    {
        [JsonProperty("files")]
        public List<ResponseItemViewModel> Files { get; set; }
    }

}