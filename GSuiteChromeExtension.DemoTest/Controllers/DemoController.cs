using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace GSuiteChromeExtension.DemoTest.Controllers
{
    [Route("api/demo")]
    [ApiController]
    public class DemoController : ControllerBase
    {

        IHostingEnvironment env;
        public DemoController(IHostingEnvironment env)
        {
            this.env = env;
        }

        [HttpPost, Route("download")]
        public async Task<IActionResult> Download([FromBody]DownloadRequestViewModel request)
        {
            var folder = Path.Combine(env.WebRootPath, "Upload");
            Directory.CreateDirectory(folder);

            var downloadingFilePath = Path.Combine(folder, request.File.Name);

            var url = string.Format(
                "https://www.googleapis.com/drive/v3/files/{0}?alt=media",
                request.File.Id);
            var httpRequest = WebRequest.CreateHttp(url);
            httpRequest.Headers.Add("Authorization", "Bearer " + request.Token);

            using (var response = (await httpRequest.GetResponseAsync()) as HttpWebResponse)
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new WebException($"Unexpected Status Code: {response.StatusCode} ({response.StatusDescription})");
                }

                using (var responseStream = response.GetResponseStream())
                {
                    using (var fileStream = new FileStream(downloadingFilePath, FileMode.Create))
                    {
                        await responseStream.CopyToAsync(fileStream);
                    }
                }
            }

            return this.Ok(new
            {
                Path = "/Upload/" + Uri.EscapeDataString(request.File.Name),
            });
        }

    }

    public class DownloadRequestViewModel
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