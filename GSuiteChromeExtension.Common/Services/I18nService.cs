using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GSuiteChromeExtension.Common.Services
{

    public interface II18nService
    {
        Dictionary<string, string> GetDefaultValues();
        Dictionary<string, string> GetValues(string languageCode);
    }

    public class I18nService : II18nService
    {
        private const string DefaultKey = "default";

        private Dictionary<string, Dictionary<string, string>> Values { get; set; }

        public I18nService(IHostingEnvironment env)
        {
            this.Values = new Dictionary<string, Dictionary<string, string>>();
            
            var languageFolder = Path.Combine(env.ContentRootPath, "wwwroot/texts/");
            this.LoadLanguageFolder(languageFolder, "texts");
        }

        public Dictionary<string, string> GetDefaultValues()
        {
            return this.GetValues(DefaultKey);
        }

        public Dictionary<string, string> GetValues(string languageCode)
        {
            if (!this.Values.TryGetValue(languageCode, out var result))
            {
                return this.GetDefaultValues();
            }

            return result;
        }

        private void LoadLanguageFolder(string folder, string prefix)
        {
            if (!Directory.Exists(folder))
            {
                return;
            }

            const string Ext = ".json";

            var defaultFile = Path.Combine(folder, prefix + Ext);
            this.Values[DefaultKey] = this.LoadLanguageFile(defaultFile, this.Values.TryGet(DefaultKey));

            var textFiles = Directory.GetFiles(folder, $"{prefix}-*{Ext}");
            var prefixLength = prefix.Length + 1;
            foreach (var textFile in textFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(textFile);
                var languageCode = fileName.Substring(prefixLength);

                this.Values[languageCode] = this.LoadLanguageFile(textFile, this.Values.TryGet(languageCode));
            }
        }

        private Dictionary<string, string> LoadLanguageFile(string filePath, Dictionary<string, string> append)
        {
            var addingItems = CommonUtils.TryReadJsonFile<Dictionary<string, string>>(filePath)
                ?? new Dictionary<string, string>();

            append = append ?? new Dictionary<string, string>();
            foreach (var addingItem in addingItems)
            {
                append[addingItem.Key] = addingItem.Value;
            }

            return append;
        }

    }

}
