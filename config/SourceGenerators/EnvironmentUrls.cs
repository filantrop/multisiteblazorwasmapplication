using All.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace SourceGenerators
{
    public class EnvironmentItem : ICloneable, IEnvironmentItem
    {
        public string? Name { get; set; }

        public string? ClientApiUrl { get; set; }
        public string? ClientUrl { get; set; }
        public string? ClientCertDomain { get; set; }

        //public EnvironmentAuth0? ClientAuth0 { get; set; }
        public string? ClientAuth_Authority { get; set; }
        public string? ClientAuth_ClientId { get; set; }

        public string? AdminAuth_Authority { get; set; }
        public string? AdminAuth_ClientId { get; set; }

        public string? AdminApiUrl { get; set; }
        public string? AdminUrl { get; set; }
        public string? AdminCertDomain { get; set; }

        //public EnvironmentAuth0? AdminAuth0 { get; set; }

        public object Clone()
        {
            var obj = (EnvironmentItem)MemberwiseClone();
            return obj;
        }


        //        "ApiClientUrl": "apiclienturl.se",
        //"ClientUrl": "clienturl.se",
        //"ApiAdminUrl": "apiadminurl.se",
        //"AdminUrl": "adminurl.se"
    }
}
