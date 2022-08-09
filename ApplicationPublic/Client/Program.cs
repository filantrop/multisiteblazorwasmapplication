using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Public.Client.Services;
using Public.Client.Domain;

namespace Public.Client
{
    
    public class Program
    {
        public static async Task Main(string[] args)
        {

            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            

            builder.Services.InitDomain(builder);
           

            // 👇 new code
            builder.Services.AddOidcAuthentication(options =>
            {
                options.ProviderOptions.Authority = AppEnvironment.ClientAuth_Authority;
                options.ProviderOptions.ClientId = AppEnvironment.ClientAuth_ClientId;
                options.ProviderOptions.AdditionalProviderParameters.Add("audience", AppEnvironment.ClientUrl);

                //builder.Configuration.Bind("ClientAuth0", options.ProviderOptions);
                options.ProviderOptions.ResponseType = "code";

                
            });
            
            var app = builder.Build();

            await app.RunAsync();
        }
    }
}

