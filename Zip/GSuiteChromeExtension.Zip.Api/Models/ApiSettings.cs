using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GSuiteChromeExtension.Zip.Api.Models
{

    public class ApiSettings
    {

        public string SevenZipExecutionPath { get; set; }
        public int MaxExecutionTime { get; set; }
        public string[] CorsDomains { get; set; }

    }

}
