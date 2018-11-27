using GSuiteChromeExtension.Common.ViewModels;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GSuiteChromeExtension.Zip.Api.Models.ViewModels
{

    public class ZipFileRequestViewModel
    {

        public List<IFormFile> Files { get; set; }
        public string Output { get; set; }

    }

    public class ZipDriveFileRequestViewModel : DefaultGoogleMultipleFilesRequest
    {

        public string Output { get; set; }

    }

    public class ZipFileResponseViewModel
    {
        public string Id { get; set; }
        public string Path { get; set; }
    }

    public class UnzipFileRequestViewModel
    {

        public IFormFile File { get; set; }

    }

    public class UnzipDriveRequestViewModel : DefaultGoogleFileRequest
    {

    }

    public class UnzipFileResponseViewModel
    {
        public string Id { get; set; }
        public FileViewModel Root { get; set; }
    }

    public class FileViewModel
    {
        public bool IsFolder { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
        public string Path { get; set; }

        public List<FileViewModel> Children { get; set; } = new List<FileViewModel>();
    }

}
