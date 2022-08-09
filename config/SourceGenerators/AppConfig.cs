using System;
using System.Collections.Generic;
using System.Text;

namespace SourceGenerators
{
    public class AppConfig
    {
        public EnvironmentItem? LocalEnvironment { get; set; } = new EnvironmentItem();
        public List<EnvironmentItem>? Environments { get; set; }=new List<EnvironmentItem>();
        public ProjectPaths? ProjectPaths { get; set; }=new ProjectPaths();
        
    }
}
