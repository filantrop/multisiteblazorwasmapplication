using System;
using System.Collections.Generic;
using System.Text;

namespace SourceGenerators
{
    public class EnvironmentAuth0:ICloneable
    {
        public string? Authority { get; set; }
        public string? ClientId { get; set; }

        public object Clone()
        {
            var obj = (EnvironmentAuth0) MemberwiseClone();
            return obj;
        }
    }
}
