using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GSuiteChromeExtension.Common
{

    public static class CommonUtils
    {

        public static string ChangeFileName(string originalFile, string newName)
        {
            var extension = Path.GetExtension(originalFile);

            return newName + extension;
        }

        public static string ChangeExtension(string originalFileName, string newExtension)
        {
            var fileName = Path.GetFileNameWithoutExtension(originalFileName);
            return fileName + newExtension;
        }

        public static T ReadJsonFile<T>(string filePath)
        {
            var fileContent = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<T>(fileContent);
        }

        public static T TryReadJsonFile<T>(string filePath)
        {
            if (File.Exists(filePath))
            {
                return CommonUtils.ReadJsonFile<T>(filePath);
            }
            else
            {
                return default(T);
            }
        }

    }

}
