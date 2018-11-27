using GSuiteChromeExtension.Common.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GSuiteChromeExtension.Common.Services
{

    public interface IGoogleDriveService
    {

        Task DownloadFileAsync(GoogleFileViewModel file, string accessToken, string location);
        Task DownloadFileAsync(GoogleFileViewModel file, string accessToken, string location, string fileName);
        Task ExportFileAsync(GoogleFileViewModel file, string accessToken, string location);
        Task ExportFileAsync(GoogleFileViewModel file, string accessToken, string location, string fileName);
        bool IsGoogleDocMimeType(string mimeType, out GoogleDriveMimeMap exportedMimeType);

    }

    public class GoogleDriveService : IGoogleDriveService
    {

        private static readonly Dictionary<string, GoogleDriveMimeMap> GoogleMimeTypes =
            GoogleDriveMimeMap.Mappings.ToDictionary(q => q.MimeType, StringComparer.OrdinalIgnoreCase);

        public async Task DownloadFileAsync(GoogleFileViewModel file, string accessToken, string location)
        {
            await this.DownloadFileAsync(file, accessToken, accessToken, null);
        }

        public async Task DownloadFileAsync(GoogleFileViewModel file, string accessToken, string location, string fileName)
        {
            if (this.IsGoogleDocMimeType(file.MimeType, out var exportMimeType))
            {
                await this.ExportFileAsync(file, accessToken, location);
                return;
            }

            var downloadFilePath = Path.Combine(location, fileName ?? file.Name);

            await this.DownloadRequestAsync(
                string.Format("https://www.googleapis.com/drive/v3/files/{0}?alt=media",
                    Uri.EscapeUriString(file.Id)),
                accessToken,
                downloadFilePath);
        }

        public async Task ExportFileAsync(GoogleFileViewModel file, string accessToken, string location)
        {
            await this.ExportFileAsync(file, accessToken, location, null);
        }

        public async Task ExportFileAsync(GoogleFileViewModel file, string accessToken, string location, string fileName)
        {
            if (!this.IsGoogleDocMimeType(file.MimeType, out var exportMimeType))
            {
                await this.DownloadFileAsync(file, accessToken, location);
                return;
            }

            var downloadFilePath = Path.Combine(location, fileName ?? file.Name + exportMimeType.Extension);

            await this.DownloadRequestAsync(
                string.Format("https://www.googleapis.com/drive/v3/files/{0}/export?alt=media&mimeType={1}",
                    Uri.EscapeUriString(file.Id),
                    Uri.EscapeDataString(exportMimeType.ExportType)),
                accessToken,
                downloadFilePath);
        }

        private async Task DownloadRequestAsync(string url, string accessToken, string filePath)
        {
            var httpRequest = WebRequest.CreateHttp(url);
            httpRequest.Headers.Add("Authorization", "Bearer " + accessToken);

            using (var response = (await httpRequest.GetResponseAsync()) as HttpWebResponse)
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new WebException($"Unexpected Status Code: {response.StatusCode} ({response.StatusDescription})");
                }

                using (var responseStream = response.GetResponseStream())
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await responseStream.CopyToAsync(fileStream);
                    }
                }
            }
        }

        public bool IsGoogleDocMimeType(string mimeType, out GoogleDriveMimeMap exportedMimeType)
        {
            return GoogleMimeTypes.TryGetValue(mimeType, out exportedMimeType);
        }

    }

}
