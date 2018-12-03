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
using NPOI.Util;
using NPOI.XWPF.UserModel;

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
            XWPFDocument doc = null;
            using (var stream = new FileStream(Path.Combine(env.WebRootPath, "Template", "Template.docx"), FileMode.Open))
            {
                doc = new XWPFDocument(stream);
            }
            //XWPFDocument doc = XWPFFactory.

            if (request.IsFolder)
            {
                var url = $"https://www.googleapis.com/drive/v3/files?fields=files(id,name,webViewLink)&q='{request.File.Id}' in parents";
                GoogleFolder folder = await GetGoogleApiResponse<GoogleFolder>(url, request.Token);

                try
                {
                    if (request.WithDescription)
                    {
                        XWPFTable table = doc.CreateTable(folder.Files.Count * request.Repetition, 2);
                        table.Width = 5072;
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

                            XWPFParagraph para = doc.CreateParagraph();
                            XWPFRun r0 = para.CreateRun();
                            r0.SetText("");
                            r0.AddBreak(BreakType.TEXTWRAPPING);
                            var rPic = para.CreateRun();
                            rPic.AddPicture(ms, (int)PictureType.PNG, file.Name, 150 * Units.EMU_PER_PIXEL, 150 * Units.EMU_PER_PIXEL);
                            rPic.AddBreak(BreakType.TEXTWRAPPING);
                            para.Alignment = ParagraphAlignment.CENTER;

                            for (int i = 0; i < request.Repetition; i++)
                            {
                                var row = table.Rows[count];
                                row.GetCell(0).SetParagraph(para);
                                var cell1P = row.GetCell(1).AddParagraph();
                                var cell1R = cell1P.CreateRun();
                                cell1R.SetText(file.Name);
                                cell1P.Alignment = ParagraphAlignment.CENTER;
                                cell1P.VerticalAlignment = TextAlignment.CENTER;
                                count++;
                            }

                            doc.RemoveBodyElement(doc.GetPosOfParagraph(para));
                        }
                    }
                    else
                    {
                        XWPFTable table = doc.CreateTable((int)Math.Ceiling(folder.Files.Count * request.Repetition / 4.0), 4);
                        var row = table.Rows[0];
                        var rowCount = 0;
                        var count = 0;

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

                            XWPFParagraph para = doc.CreateParagraph();
                            XWPFRun r0 = para.CreateRun();
                            r0.SetText("");
                            r0.AddBreak(BreakType.TEXTWRAPPING);
                            var rPic = para.CreateRun();
                            rPic.AddPicture(ms, (int)PictureType.PNG, file.Name, 150 * Units.EMU_PER_PIXEL, 150 * Units.EMU_PER_PIXEL);
                            rPic.AddBreak(BreakType.TEXTWRAPPING);
                            var r1 = para.CreateRun();
                            r1.SetText(file.Name);
                            para.Alignment = ParagraphAlignment.CENTER;

                            for (var i = 0; i < request.Repetition; i++)
                            {
                                if (count >= 4)
                                {
                                    count = 0;
                                    row = table.Rows[++rowCount];
                                }

                                row.GetCell(count).SetParagraph(para);
                                count++;
                            }

                            doc.RemoveBodyElement(doc.GetPosOfParagraph(para));
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("wtf: " + e.Message);
                }
            }
            else
            {
                var url = $"https://www.googleapis.com/drive/v3/files/{request.File.Id}?fields=id,name,webViewLink";
                ResponseItemViewModel item = await GetGoogleApiResponse<ResponseItemViewModel>(url, request.Token);

                try
                {
                    //XWPFDocument doc = new XWPFDocument();
                    //XWPFParagraph para = doc.CreateParagraph();
                    //XWPFRun r0 = para.CreateRun();
                    //r0.SetText("Title1");
                    //para.BorderTop = Borders.Thick;
                    //para.FillBackgroundColor = "EEEEEE";
                    //para.FillPattern = NPOI.OpenXmlFormats.Wordprocessing.ST_Shd.diagStripe;


                    //table.GetRow(1).GetCell(1).SetText("EXAMPLE OF TABLE");

                    //XWPFTableCell c1 = table.GetRow(0).GetCell(0);
                    //XWPFParagraph p1 = c1.AddParagraph();   //don't use doc.CreateParagraph
                    //XWPFRun r1 = p1.CreateRun();
                    //r1.SetText("The quick brown fox");
                    //r1.IsBold = true;

                    //r1.FontFamily = "Courier";
                    //r1.SetUnderline(UnderlinePatterns.DotDotDash);
                    //r1.SetTextPosition(100);
                    //c1.SetColor("FF0000");


                    //table.GetRow(2).GetCell(2).SetText("only text");

                    byte[] imageData = null;
                    MemoryStream ms;

                    ms = null;

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

                    if (request.WithDescription)
                    {
                        XWPFParagraph para = doc.CreateParagraph();
                        XWPFRun r0 = para.CreateRun();
                        r0.SetText("");
                        r0.AddBreak(BreakType.TEXTWRAPPING);
                        var rPic = para.CreateRun();
                        rPic.AddPicture(ms, (int)PictureType.PNG, request.File.Name, 150 * Units.EMU_PER_PIXEL, 150 * Units.EMU_PER_PIXEL);
                        rPic.AddBreak(BreakType.TEXTWRAPPING);
                        para.Alignment = ParagraphAlignment.CENTER;

                        XWPFTable table = doc.CreateTable(request.Repetition, 2);
                        table.Width = 5072;

                        foreach (var row in table.Rows)
                        {
                            row.GetCell(0).SetParagraph(para);
                            var cell1P = row.GetCell(1).AddParagraph();
                            var cell1R = cell1P.CreateRun();
                            cell1R.SetText(item.Name);
                            cell1P.Alignment = ParagraphAlignment.CENTER;
                            cell1P.VerticalAlignment = TextAlignment.CENTER;
                        }

                        doc.RemoveBodyElement(doc.GetPosOfParagraph(para));
                    }
                    else
                    {
                        XWPFParagraph para = doc.CreateParagraph();
                        XWPFRun r0 = para.CreateRun();
                        r0.SetText("");
                        r0.AddBreak(BreakType.TEXTWRAPPING);
                        var rPic = para.CreateRun();
                        rPic.AddPicture(ms, (int)PictureType.PNG, request.File.Name, 150 * Units.EMU_PER_PIXEL, 150 * Units.EMU_PER_PIXEL);
                        rPic.AddBreak(BreakType.TEXTWRAPPING);
                        var r1 = para.CreateRun();
                        r1.SetText(request.File.Name);
                        para.Alignment = ParagraphAlignment.CENTER;

                        XWPFTable table = doc.CreateTable((int)Math.Ceiling(request.Repetition / 4.0), 4);
                        var count = 0;

                        for (int i = 0; i < table.Rows.Count && count < request.Repetition; i++)
                        {
                            for (int j = 0; j < 4 && count < request.Repetition; j++)
                            {
                                table.GetRow(i).GetCell(j).SetParagraph(para);
                                count++;
                            }
                        }

                        doc.RemoveBodyElement(doc.GetPosOfParagraph(para));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("wtf: " + e.Message);
                }
            }

            var path = Path.Combine(env.WebRootPath, "Upload");
            Directory.CreateDirectory(path);

            FileStream out1 = new FileStream(Path.Combine(path, $"{request.File.Id}.docx"), FileMode.Create);
            doc.Write(out1);
            out1.Close();

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