using GSuiteChromeExtension.Common.Services;
using GSuiteChromeExtension.Common.ViewModels;
using GSuiteChromeExtension.Zip.Api.Models.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using ServiceSharp;
using ServiceSharp.DI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GSuiteChromeExtension.Zip.Api.Models.Services
{

    public interface IZipBrowserService : IService
    {
        string Id { get; }
        string WorkingFolder { get; }
        string ResultFolder { get; }
        string WorkingFile { get; }
        string Initialize();
        void Initialize(string id, bool throwIfNotExist);
        Task<string> SaveFileAsync(IFormFile file);
        Task SaveDriveFilesAsync(string oauth, IEnumerable<GoogleFileViewModel> files);
        Task UnzipAsync(string workingFile = null);
        FileViewModel List(string path);
        string GetFileAndFolderSystemPath(string path, bool throwIfFolder, bool throwIfFile);
        Task<string> ZipAsync(string outputType);
    }

    [ServiceLifetime(ServiceDILifetime.Instance)]
    public class ZipBrowserService : IZipBrowserService, IService<IZipBrowserService>
    {
        IHostingEnvironment hostingEnvironment;
        ISevenZipService sevenZipService;
        IGoogleDriveService googleDriveService;

        public string Id { get; private set; }

        public string WorkingFolder
        {
            get
            {
                if (this.Id == null)
                {
                    throw new InvalidOperationException("Browser is not initialized.");
                }

                return Path.Combine(this.hostingEnvironment.ContentRootPath, "Uploads/Zip", this.Id);
            }
        }

        public string ResultFolder
        {
            get
            {
                return Path.Combine(this.WorkingFolder, "Result");
            }
        }

        public string WorkingFile
        {
            get
            {
                return Directory.GetFiles(this.WorkingFolder).FirstOrDefault();
            }
        }

        public ZipBrowserService(IHostingEnvironment hostingEnvironment, ISevenZipService sevenZipService, IGoogleDriveService googleDriveService)
        {
            this.hostingEnvironment = hostingEnvironment;
            this.sevenZipService = sevenZipService;
            this.googleDriveService = googleDriveService;
        }

        public string Initialize()
        {
            var id = Guid.NewGuid().ToString();
            this.Initialize(id, false);
            return id;
        }

        public void Initialize(string id, bool throwIfNotExist)
        {
            this.Id = id;

            var workingFolder = this.WorkingFolder;
            if (throwIfNotExist && !Directory.Exists(workingFolder))
            {
                throw new IOException("ID Not found");
            }

            Directory.CreateDirectory(workingFolder);
        }

        public async Task<string> SaveFileAsync(IFormFile file)
        {
            return await file.SaveFileAsync(this.WorkingFolder);
        }

        public async Task SaveDriveFilesAsync(string oauth, IEnumerable<GoogleFileViewModel> files)
        {
            foreach (var file in files)
            {
                await this.googleDriveService.DownloadFileAsync(
                    file,
                    oauth,
                    this.WorkingFolder);
            }
        }

        public async Task UnzipAsync(string workingFile = null)
        {
            workingFile = workingFile ?? this.WorkingFile;

            await this.sevenZipService.UnzipAsync(workingFile, this.ResultFolder);
        }

        public async Task<string> ZipAsync(string outputType)
        {
            var outputFileName = "output." + outputType;
            var outputFilePath = Path.Combine(this.ResultFolder, outputFileName);

            var files = Directory.GetFiles(this.WorkingFolder);
            await this.sevenZipService.ZipAsync(files, outputFilePath);

            return outputFileName;
        }

        public FileViewModel List(string path)
        {
            var folder = this.GetFileAndFolderSystemPath(path, false, true);

            var result = new FileViewModel()
            {
                IsFolder = true,
                Name = Path.GetFileName(folder),
                Path = path,
            };

            var subFolders = Directory.GetDirectories(folder);
            foreach (var subFolder in subFolders)
            {
                var name = Path.GetFileName(subFolder);
                result.Children.Add(new FileViewModel()
                {
                    IsFolder = true,
                    Name = name,
                    Path = $"{path}\\{name}",
                });
            }

            var files = Directory.GetFiles(folder);
            foreach (var file in files)
            {
                var name = Path.GetFileName(file);
                result.Children.Add(new FileViewModel()
                {
                    IsFolder = false,
                    Name = name,
                    Extension = Path.GetExtension(name),
                    Path = $"{path}\\{name}",
                });
            }

            return result;
        }

        public string GetFileAndFolderSystemPath(string path, bool throwIfFolder, bool throwIfFile)
        {
            var result = this.ResultFolder;

            if (!string.IsNullOrEmpty(path))
            {
                path = path.Replace("/", @"\");

                if (path.StartsWith(@"\"))
                {
                    path = path.Substring(1);
                }

                if (path.StartsWith('\\') || path.Contains("..\\"))
                {
                    throw new InvalidDataException("Forbidden Path.");
                }
                
                result = Path.Combine(result, path);
            }

            if (Directory.Exists(result) )
            {
                if (throwIfFolder)
                {
                    throw new InvalidDataException("This is a folder path.");
                }

                return result;
            }

            if (File.Exists(result))
            {
                if (throwIfFile)
                {
                    throw new InvalidDataException("This is a file path.");
                }

                return result;
            }

            throw new IOException("File or Folder not found.");
        }

    }

}
