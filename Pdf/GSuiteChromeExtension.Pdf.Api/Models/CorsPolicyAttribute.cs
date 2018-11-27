using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Cors;
using System.Web.Http.Cors;

namespace GSuiteChromeExtension.Pdf.Api.Models
{
    public class CorsPolicyAttribute : Attribute, ICorsPolicyProvider
    {

        private CorsPolicy _policy;

        public CorsPolicyAttribute()
        {
            // Create a CORS policy.
            _policy = new CorsPolicy()
            {
                AllowAnyMethod = true,
                AllowAnyHeader = true,
            };

            // Add allowed origins.
            var allowedDomains = ConfigurationManager.AppSettings["CorsHosts"]
                .Split(';');

            foreach (var domain in allowedDomains)
            {
                _policy.Origins.Add(domain);
            }
        }

        public Task<CorsPolicy> GetCorsPolicyAsync(HttpRequestMessage request)
        {
            return Task.FromResult(_policy);
        }

        public Task<CorsPolicy> GetCorsPolicyAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return this.GetCorsPolicyAsync(request);
        }
    }
}