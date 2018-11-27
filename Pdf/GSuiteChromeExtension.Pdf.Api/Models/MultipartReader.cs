using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace GSuiteChromeExtension.Pdf.Api.Models
{

    public class MultipartReader : MultipartMemoryStreamProvider
    {
        public Dictionary<string, List<HttpContent>> Values { get; } = new Dictionary<string, List<HttpContent>>(StringComparer.OrdinalIgnoreCase);

        public override Task ExecutePostProcessingAsync()
        {
            foreach (var content in Contents)
            {
                var name = content.Headers.ContentDisposition.Name;

                // Remove the quote
                name = name.Substring(1, name.Length - 2);

                if (!this.Values.TryGetValue(name, out var values))
                {
                    values = new List<HttpContent>();
                    this.Values[name] = values;
                }

                values.Add(content);
            }

            return base.ExecutePostProcessingAsync();
        }

        public async Task<string> TryReadAsync(string name)
        {
            if (!this.Values.TryGetValue(name, out var resultList) || resultList.Count == 0)
            {
                return null;
            }

            return await resultList[0].ReadAsStringAsync();
        }

    }
}