using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GSuiteChromeExtension.Common.Services;
using GSuiteChromeExtension.Zip.Api.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServiceSharp.AspNetCore;

namespace GSuiteChromeExtension.Zip.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
                });

            var settings = new ApiSettings();
            this.Configuration.Bind(settings);
            services.AddSingleton(settings);

            services.AddServices()
                .AddScoped<IGoogleDriveService, GoogleDriveService>();

            services.AddCors();

        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ApiSettings apiSettings)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            apiSettings.SevenZipExecutionPath = Path.Combine(
                env.ContentRootPath,
                @"Libs\7zip\x64\7za.exe")
                .Replace("/", @"\");

            app.UseCors(builder =>
            {
                builder.WithOrigins(apiSettings.CorsDomains)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });

            app.UseMvc();
        }
    }
}
