using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GSuiteChromeExtension.Common.ViewModels;
using GSuiteChromeExtension.Zip.Api.Models;
using GSuiteChromeExtension.Zip.Api.Models.Services;
using GSuiteChromeExtension.Zip.Api.Models.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GSuiteChromeExtension.Zip.Api.Controllers
{

    [Route("api/zip")]
    [ApiController]
    public class ZipController : ControllerBase
    {
        private const string DefaultOutputExtension = "zip";
        IZipBrowserService zipBrowserService;
        public ZipController(IZipBrowserService zipBrowserService)
        {
            this.zipBrowserService = zipBrowserService;
        }

        [HttpPost, Route("zip")]
        public async Task<ActionResult<ZipFileResponseViewModel>> Zip([FromForm]ZipFileRequestViewModel request)
        {
            if (request == null || request.Files == null)
            {
                return this.BadRequest();
            }

            request.Output = request.Output ?? DefaultOutputExtension;

            this.zipBrowserService.Initialize();
            foreach (var file in request.Files)
            {
                await this.zipBrowserService.SaveFileAsync(file);
            }

            return await this.ZipAndReturnAsync(request.Output);
        }

        [HttpPost, Route("zip-drive")]
        public async Task<ActionResult<ZipFileResponseViewModel>> ZipDrive(ZipDriveFileRequestViewModel request)
        {
            if (request == null || request.OAuthToken == null || request.Files == null || request.Files.Count() == 0)
            {
                return this.BadRequest();
            }

            request.Output = request.Output ?? DefaultOutputExtension;

            this.zipBrowserService.Initialize();
            await this.zipBrowserService.SaveDriveFilesAsync(request.OAuthToken, request.Files);

            return await this.ZipAndReturnAsync(request.Output);
        }

        [HttpPost, Route("unzip")]
        public async Task<ActionResult<UnzipFileResponseViewModel>> Unzip([FromForm]UnzipFileRequestViewModel request)
        {
            if (request == null || request.File == null)
            {
                return this.BadRequest();
            }

            this.zipBrowserService.Initialize();
            await this.zipBrowserService.SaveFileAsync(request.File);
            await this.zipBrowserService.UnzipAsync();
            var rootContent = this.zipBrowserService.List(null);

            return new UnzipFileResponseViewModel()
            {
                Id = this.zipBrowserService.Id,
                Root = rootContent,
            };
        }

        [HttpPost, Route("unzip-drive")]
        public async Task<ActionResult<UnzipFileResponseViewModel>> UnzipDrive(UnzipDriveRequestViewModel request)
        {
            if (request == null || request.File == null)
            {
                return this.BadRequest();
            }

            this.zipBrowserService.Initialize();
            await this.zipBrowserService.SaveDriveFilesAsync(request.OAuthToken, new GoogleFileViewModel[] { request.File, });
            await this.zipBrowserService.UnzipAsync();
            var rootContent = this.zipBrowserService.List(null);

            return new UnzipFileResponseViewModel()
            {
                Id = this.zipBrowserService.Id,
                Root = rootContent,
            };
        }

        [HttpGet, Route("list")] 
        public ActionResult<FileViewModel> List(string id, string path)
        {
            if (!this.InitializeExistId(id, out var error))
            {
                return error;
            };

            try
            {
                return this.zipBrowserService.List(path);
            }
            catch (Exception ex)
            {
                return this.BadRequest(ex.Message);
            }
        }

        [HttpGet, Route("download")]
        public IActionResult Download(string id, string path)
        {
            if (!this.InitializeExistId(id, out var error))
            {
                return error;
            };

            try
            {
                var filePath = this.zipBrowserService.GetFileAndFolderSystemPath(path, true, false);
                return this.PhysicalFile(filePath, "application/octet-stream", Path.GetFileName(filePath));
            }
            catch (Exception ex)
            {
                return this.NotFound(ex.Message);
            }
        }

        private bool InitializeExistId(string id, out ActionResult error)
        {
            if (id == null)
            {
                error = this.BadRequest();
                return false;
            }

            try
            {
                this.zipBrowserService.Initialize(id, true);
            }
            catch (IOException ex)
            {
                error = this.NotFound(ex.Message);
                return false;
            }

            error = null;
            return true;
        }

        private async Task<ZipFileResponseViewModel> ZipAndReturnAsync(string outputType)
        {
            var filePath = await this.zipBrowserService.ZipAsync(outputType);

            return new ZipFileResponseViewModel()
            {
                Id = this.zipBrowserService.Id,
                Path = filePath,
            };
        }

        private async Task<UnzipFileResponseViewModel> UnzipAndReturnAsync()
        {
            await this.zipBrowserService.UnzipAsync();
            var rootContent = this.zipBrowserService.List(null);

            return new UnzipFileResponseViewModel()
            {
                Id = this.zipBrowserService.Id,
                Root = rootContent,
            };
        }

    }

}