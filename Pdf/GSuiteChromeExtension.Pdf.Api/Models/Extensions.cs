using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace GSuiteChromeExtension.Pdf.Api.Models
{

    internal static class Extensions
    {

        public static string GetFileName(this HttpContent httpContent)
        {
            var result = httpContent.Headers.ContentDisposition.FileName;

            return result.Substring(1, result.Length - 2);
        }

    }

}