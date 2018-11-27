using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GSuiteChromeExtension.Zip.Web.Models
{
    public class WebSettings
    {

        public GoogleSettings Google { get; set; }
        public string ApiServer { get; set; }

        public class GoogleSettings
        {
            public string ClientId { get; set; }
        }

    }
}
