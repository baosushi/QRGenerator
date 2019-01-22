using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GSuiteChromeExtension.DjvuViewer.Web.Models
{

    public class WebSettings
    {
        public GoogleSettings Google { get; set; }

        public class GoogleSettings
        {
            public string ClientId { get; set; }
            public string DeveloperKey { get; set; }
            public string ProjectNumber { get; set; }
        }

    }

}
