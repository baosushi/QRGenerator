using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace GSuiteChromeExtension.Common.Services
{
    public interface ILocalizationService
    {
        string GetRawText(string key);
        HtmlString GetText(string key);
        HtmlString GetText(string key, params object[] formatParameters);
        string GetLanguageCode();
    }

    public class LocalizationService : ILocalizationService
    {

        Dictionary<string, string> fallbackValues;
        Dictionary<string, string> values;
        string languageCode;

        public LocalizationService(IHttpContextAccessor httpContextAccessor, II18nService i18nService)
        {
            this.GetLanguage(httpContextAccessor.HttpContext);
            this.values = i18nService.GetValues(this.languageCode);
            this.fallbackValues = i18nService.GetDefaultValues();
        }

        public HtmlString GetText(string key)
        {
            return new HtmlString(this.GetRawText(key));
        }

        public HtmlString GetText(string key, params object[] formatParameters)
        {
            return new HtmlString(this.GetRawText(key).Format(formatParameters));
        }

        public string GetRawText(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return "";
            }
            else
            {
                return this.values.TryGet(key) ?? this.fallbackValues.TryGet(key) ?? key;
            }
        }

        public string GetLanguageCode()
        {
            return this.languageCode;
        }

        private void GetLanguage(HttpContext context)
        {
            var path = context.Request.Path.Value;

            const string hlFormat = "/hl-";
            string hl = null;

            // From Url
            var urlHl = path.IndexOf(hlFormat);
            if (urlHl > -1)
            {
                var len = hlFormat.Length;

                var urlHlEnd = path.IndexOf("/", urlHl + 1);
                if (urlHlEnd == -1)
                {
                    urlHlEnd = path.Length;
                }

                hl = path.Substring(urlHl + len, urlHlEnd - urlHl - len);
            }

            // From Cookie
            if (hl.IsNullOrEmpty())
            {
                hl = context.Request.Cookies
                    .FirstOrDefault(q => q.Key == "hl")
                    .Value;
            }

            // Set Default
            hl = hl ?? "en";

            context.Response.Cookies.Append("hl", hl);

            CultureInfo culture;
            try
            {
                culture = new CultureInfo(hl);
            }
            catch (CultureNotFoundException)
            {
                culture = CultureInfo.CurrentCulture;
            }

            context.Items["LanguageCode"] = this.languageCode = hl;
            context.Items["Culture"] = culture;
        }

    }
}
