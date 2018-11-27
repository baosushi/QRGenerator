using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Http
{
    public static class HttpExtensions
    {

        public static async Task<string> SaveFileAsync(this IFormFile formFile, string folder)
        {
            return await SaveFileAsync(formFile, folder, formFile.FileName);
        }

        public static async Task<string> SaveFileAsync(this IFormFile formFile, string folder, string fileName)
        {
            var filePath = Path.Combine(folder, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await formFile.CopyToAsync(fileStream);
            }

            return filePath;
        }

    }
}
