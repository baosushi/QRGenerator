using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GSuiteChromeExtension.Pdf.Web.Models
{

    public class WebSettings
    {

        public string ApiServer { get; set; }
        public GoogleSettings Google { get; set; }

        public class GoogleSettings
        {
            public string ClientId { get; set; }
        }

    }

}
