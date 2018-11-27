using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace GSuiteChromeExtension.Pdf.Api.Controllers
{
    public class HomeController : ApiController
    {

        [HttpGet, Route("")]
        public IHttpActionResult Index()
        {
#if DEBUG
            return this.Content(HttpStatusCode.OK, "Should redirect when Release.");
#else
            return this.Redirect("https://www.pdfconverted.com/");
#endif
        }


    }

}
