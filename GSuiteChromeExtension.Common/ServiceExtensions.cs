using GSuiteChromeExtension.Common.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class ServiceExtensions
    {

        public static IServiceCollection AddI18n(this IServiceCollection services)
        {
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<II18nService, I18nService>();
            services.AddScoped<ILocalizationService, LocalizationService>();

            return services;
        }

    }
}
