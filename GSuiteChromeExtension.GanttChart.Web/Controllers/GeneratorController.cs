using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace GSuiteChromeExtension.GanttChart.Web.Controllers
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
        public IActionResult Generate([FromBody]GenerateRequestViewModel request)
        {
            var fileName = Guid.NewGuid().ToString() + ".xml";
            var path = Path.Combine(env.WebRootPath, "Upload");
            Directory.CreateDirectory(path);

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(request.XmlContent);
            doc.Save(Path.Combine(env.WebRootPath, "Upload", fileName));

            return this.Ok(new
            {
                Path = "/Upload/" + Uri.EscapeDataString(fileName),
                FileName = fileName
            });
        }

        [HttpPost, Route("delete")]
        public IActionResult Delete([FromBody]DeleteRequestViewModel request)
        {
            try
            {
                System.IO.File.Delete(Path.Combine(env.WebRootPath, "Upload", request.FileName));

                return this.Ok();
            }
            catch (Exception)
            {
                return this.BadRequest();
            }
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

    public class GenerateRequestViewModel
    {
        public string XmlContent { get; set; }
    }

    public class DeleteRequestViewModel
    {
        public string FileName { get; set; }
    }
}