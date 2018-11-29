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
            if (request.IsFolder)
            {
                var url = $"https://www.googleapis.com/drive/v3/files?fields=files(id,name,webViewLink)&q='{request.File.Id}' in parents";
            }
            else
            {
                var url = $"https://www.googleapis.com/drive/v3/files/{request.File.Id}?fields=id,name,webViewLink";
                ResponseItemViewModel item = null;
                var httpRequest = WebRequest.CreateHttp(url);
                httpRequest.Headers.Add("Authorization", "Bearer " + request.Token);

                using (var response = (await httpRequest.GetResponseAsync()) as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new WebException($"Unexpected Status Code: {response.StatusCode} ({response.StatusDescription})");
                    }

                    using(var reader = new StreamReader(response.GetResponseStream()))
                    {
                        item = JsonConvert.DeserializeObject<ResponseItemViewModel>(reader.ReadToEnd());
                    }
                }

                try
                {
                    XWPFDocument doc = new XWPFDocument();
                    XWPFParagraph para = doc.CreateParagraph();
                    XWPFRun r0 = para.CreateRun();
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

                    r0.SetText("");
                    r0.AddBreak(BreakType.TEXTWRAPPING);
                    var rPic = para.CreateRun();
                    rPic.AddPicture(ms, (int)PictureType.PNG, request.File.Name, 150 * Units.EMU_PER_PIXEL, 150 * Units.EMU_PER_PIXEL);
                    rPic.AddBreak(BreakType.TEXTWRAPPING);
                    var r1 = para.CreateRun();
                    r1.SetText(request.File.Name);
                    para.Alignment = ParagraphAlignment.CENTER;

                    XWPFTable table = doc.CreateTable(3, 3);
                    
                    table.GetRow(0).GetCell(1).SetParagraph(para);

                    doc.RemoveBodyElement(doc.GetPosOfParagraph(para));
                    

                    FileStream out1 = new FileStream("simpleTable.docx", FileMode.Create);
                    doc.Write(out1);
                    out1.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine("wtf: " + e.Message);
                }
            }


            var folder = Path.Combine(env.WebRootPath, "Upload");
            Directory.CreateDirectory(folder);

            var downloadingFilePath = Path.Combine(folder, request.File.Name);

            //var url = string.Format(
            //    //"https://www.googleapis.com/drive/v3/files/{0}?alt=media",
            //    "https://www.googleapis.com/drive/v3/files/{0}",
            //    request.File.Id);
            //var httpRequest = WebRequest.CreateHttp(url);
            //httpRequest.Headers.Add("Authorization", "Bearer " + request.Token);

            //using (var response = (await httpRequest.GetResponseAsync()) as HttpWebResponse)
            //{
            //    if (response.StatusCode != HttpStatusCode.OK)
            //    {
            //        throw new WebException($"Unexpected Status Code: {response.StatusCode} ({response.StatusDescription})");
            //    }

            //    using (var responseStream = response.GetResponseStream())
            //    {
            //        using (var fileStream = new FileStream(downloadingFilePath, FileMode.Create))
            //        {
            //            await responseStream.CopyToAsync(fileStream);
            //        }
            //    }
            //}

            return this.Ok(new
            {
                Path = "/Upload/" + Uri.EscapeDataString(request.File.Name),
            });
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

}