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

namespace GSuiteChromeExtension.GifMaker.Web.Controllers
{
    [Route("api/file")]
    [ApiController]
    public class FileController : ControllerBase
    {
        IHostingEnvironment env;
        public FileController(IHostingEnvironment env)
        {
            this.env = env;
        }

        [HttpGet, Route("getFile")]
        public async Task<IActionResult> GetFile(string Token, string Id, string Name)
        {
            var url = $"https://www.googleapis.com/drive/v3/files/{Id}?alt=media";
            //var item = await GetGoogleApiResponse<Stream>(url, Token);

            var data = new byte[] { };
            using (var wc = new WebClient())
            {
                wc.Headers.Add("Authorization", "Bearer " + Token);
                data = wc.DownloadData(url);
            }

            var ms = new MemoryStream(data);
            ms.Seek(0, SeekOrigin.Begin);
            var item = ms.ToArray();

            //try
            //{
            //    byte[] imageData = null;
            //    MemoryStream ms = null;

            //    try
            //    {
            //        using (var wc = new WebClient())
            //        {
            //            imageData = wc.DownloadData($"https://chart.googleapis.com/chart?chs=150x150&cht=qr&chl={item.WebViewLink}");
            //        }
            //        ms = new MemoryStream(imageData);
            //    }
            //    catch (Exception ex)
            //    {
            //        //forbidden, proxy issues, file not found (404) etc
            //    }
            //}
            //catch (Exception e)
            //{
            //    return this.StatusCode(500, e.Message);
            //}

            //var path = Path.Combine(env.WebRootPath, "Upload");
            //Directory.CreateDirectory(path);

            return File(item, "video/mp4");

            //return this.Ok(new
            //{
            //    Path = "/Upload/" + Uri.EscapeDataString($"{request.File.Id}.docx"),
            //});
        }

        private async Task<byte[]> GetGoogleApiResponse<T>(string url, string token)
        {
            Stream item = null;
            var httpRequest = WebRequest.CreateHttp(url);
            httpRequest.Headers.Add("Authorization", "Bearer " + token);

            using (var response = (await httpRequest.GetResponseAsync()) as HttpWebResponse)
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new WebException($"Unexpected Status Code: {response.StatusCode} ({response.StatusDescription})");
                }

                item = response.GetResponseStream();
            }

            using (var memoryStream = new MemoryStream())
            {
                item.CopyTo(memoryStream);
                
                return memoryStream.ToArray();
            }
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
    }

    public class GoogleFile
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}