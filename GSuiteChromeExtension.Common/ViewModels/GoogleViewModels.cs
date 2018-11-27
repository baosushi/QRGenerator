using System;
using System.Collections.Generic;
using System.Text;

namespace GSuiteChromeExtension.Common.ViewModels
{

    public class DefaultGoogleFileRequest
    {
        public GoogleFileViewModel File { get; set; }
        public string OAuthToken { get; set; }
    }

    public class DefaultGoogleMultipleFilesRequest
    {
        public IEnumerable<GoogleFileViewModel> Files { get; set; }
        public string OAuthToken { get; set; }
    }

    public class GoogleFileViewModel
    {
        public string Id { get; set; }
        public string MimeType { get; set; }
        public string Name { get; set; }
    }


    public class GoogleDriveMimeMap
    {

        public static List<GoogleDriveMimeMap> Mappings = new List<GoogleDriveMimeMap>()
        {
            new GoogleDriveMimeMap("application/vnd.google-apps.document", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", ".docx"),
            new GoogleDriveMimeMap("application/vnd.google-apps.spreadsheet", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", ".xlsx"),
            new GoogleDriveMimeMap("application/vnd.google-apps.drawing", "image/png", ".png"),
            new GoogleDriveMimeMap("application/vnd.google-apps.presentation", "application/vnd.openxmlformats-officedocument.presentationml.presentation", ".xlsx"),
            new GoogleDriveMimeMap("application/vnd.google-apps.script", "application/vnd.google-apps.script+json", ".json"),
        };

        public string MimeType { get; set; }
        public string ExportType { get; set; }
        public string Extension { get; set; }

        public GoogleDriveMimeMap() { }

        public GoogleDriveMimeMap(string mimeType)
        {
            this.MimeType = mimeType;
        }

        public GoogleDriveMimeMap(string mimeType, string exportType, string extension)
        {
            this.MimeType = mimeType;
            this.ExportType = exportType;
            this.Extension = extension;
        }

    }

}
