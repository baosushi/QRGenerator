using ServiceSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSuiteChromeExtension.Zip.Api.Models.Services
{

    public interface ISevenZipService : IService
    {
        Task ZipAsync(IEnumerable<string> inputPaths, string outputFilePath);
        Task UnzipAsync(string inputPaths, string outputFolderPath);

    }

    public class SevenZipService : ISevenZipService, IService<ISevenZipService>
    {
        private const string ErrorZipping = "There was a problem with zipping.";
        private const string ErrorUnzipping = "There was a problem with unzipping.";

        ApiSettings apiSettings;
        public SevenZipService(ApiSettings apiSettings)
        {
            this.apiSettings = apiSettings;
        }

        public async Task ZipAsync(IEnumerable<string> inputPaths, string outputFilePath)
        {
            await Task.Run(() =>
            {
                var arguments = new StringBuilder();

                arguments.Append($"a \"{outputFilePath}\"");

                foreach (var input in inputPaths)
                {
                    arguments.Append($" \"{input}\"");
                }

                var processStartInfo = new ProcessStartInfo(
                    this.apiSettings.SevenZipExecutionPath,
                    arguments.ToString())
                {
                    CreateNoWindow = true,
                };

                var process = Process.Start(processStartInfo);
                if (!process.WaitForExit(this.apiSettings.MaxExecutionTime))
                {
                    try
                    {
                        process.Kill();
                    }
                    catch (Exception) { }

                    throw new OperationCanceledException(ErrorZipping);
                }

                if (!File.Exists(outputFilePath))
                {
                    throw new OperationCanceledException(ErrorZipping);
                }
            });
        }

        public async Task UnzipAsync(string inputPaths, string outputFolderPath)
        {
            await Task.Run(() =>
            {
                Directory.CreateDirectory(outputFolderPath);

                inputPaths = inputPaths.Replace("/", @"\");
                outputFolderPath = outputFolderPath.Replace("/", @"\");

                var arguments = $"x \"{inputPaths}\" -o\"{outputFolderPath}\"";

                var processStartInfo = new ProcessStartInfo(
                    this.apiSettings.SevenZipExecutionPath,
                    arguments)
                {
                    CreateNoWindow = true,
                };

                var process = Process.Start(processStartInfo);
                if (!process.WaitForExit(this.apiSettings.MaxExecutionTime))
                {
                    try
                    {
                        process.Kill();
                    }
                    catch (Exception) { }

                    throw new OperationCanceledException(ErrorUnzipping);
                }
            });
        }

    }

}
