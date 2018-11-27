using System;
using System.Collections.Generic;
using System.Text;

namespace System.Collections.Generic
{
    public static partial class CommonExtensions
    {


        public static TValue TryGet<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
        {
            dict.TryGetValue(key, out var result);
            return result;
        }


    }
}

namespace System
{

    public static partial class CommonExtensions
    {
        public static string Format(this string str, params object[] parameters)
        {
            return string.Format(str, parameters);
        }

        public static bool IsNullOrEmpty(this string input)
        {
            return string.IsNullOrEmpty(input);
        }

    }

}

namespace System.Globalization
{
    public static partial class CommonExtensions
    {
        public static string TwoLetterOrSpecial(this CultureInfo cultureInfo)
        {
            switch (cultureInfo.Name.ToLower())
            {
                case "fil":
                    return "tl";
                case "no":
                    return "no";
                case "zh-cn":
                    return "zh-CN";
                case "zh-tw":
                    return "zh-TW";
                default:
                    return cultureInfo.TwoLetterISOLanguageName;
            }
        }
    }
}