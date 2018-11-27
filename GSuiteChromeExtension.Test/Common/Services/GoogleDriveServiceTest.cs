using GSuiteChromeExtension.Common.Services;
using GSuiteChromeExtension.Common.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace GSuiteChromeExtension.Test.Common.Services
{

    public class GoogleDriveServiceTest
    {

        [Fact]
        public async void DownloadFileAsyncTest1()
        {
            var driveService = new GoogleDriveService();
            await driveService.DownloadFileAsync(
                new GoogleFileViewModel()
                {
                    Id = "1rIo43NYHEvZEu9aXfUkq2FzcR4ufR-uK",
                    MimeType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    Name = "test.docx",
                },
                "ya29.GlxMBsFlMjKwWwVkOID5Ac8GYUdgex-5ZGbyR_UUveaJE8K9NSVxaTltuChdOuzIzTe1CLX90bH-nlE45aWA3_l8gQ0Vsg13iKRQCeiaNR88U9w_x7_hE7-32zz09g",
                @"D:\Temp\");
        }

        [Fact]
        public async void DownloadFileAsyncTest2()
        {
            var driveService = new GoogleDriveService();
            await driveService.DownloadFileAsync(
                new GoogleFileViewModel()
                {
                    Id = "1ANJSeu6oq09ycZNXRupyiljorVCaZEj1LpRs8Nrz0cg",
                    MimeType = "application/vnd.google-apps.document",
                    Name = "Test",
                },
                "ya29.GlxMBsFlMjKwWwVkOID5Ac8GYUdgex-5ZGbyR_UUveaJE8K9NSVxaTltuChdOuzIzTe1CLX90bH-nlE45aWA3_l8gQ0Vsg13iKRQCeiaNR88U9w_x7_hE7-32zz09g",
                @"D:\Temp\");
        }

    }

}
