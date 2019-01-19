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

namespace GSuiteChromeExtension.FreeFlashPlayer.Web.Controllers
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
        public async Task<IActionResult> GetFile(string Token, string Id)
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

            return File(item, "application/x-shockwave-flash");
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