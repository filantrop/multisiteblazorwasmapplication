using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Refit;
using Microsoft.Extensions.DependencyInjection;
using Public.Client.Api;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Public.Client.Domain;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.Net.Http;

namespace Public.Client.Services
{

    public static class DomainStartup
    {
        public static void InitDomain(this IServiceCollection services, WebAssemblyHostBuilder builder)
        {
            //services.AddScoped<IAdminService, AdminService>();

            Uri uri = null;
            var configuration = builder.Configuration;

            //var ClientApiUrl = configuration["ClientApiUrl"];
            var ClientApiUrl = AppEnvironment.ClientApiUrl;

            //var c = configuration["ClientAuth0:ClientId"];
            uri = new Uri(ClientApiUrl);
           
            //services.AddRefitClient<IAdminApi>()
            //        .ConfigureHttpClient(c => c.BaseAddress = uri)
            //        .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>()
            //        //.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>())
            //        //.CreateClient("ServerAPI"));
            //        .SetHandlerLifetime(TimeSpan.FromMinutes(2));

            //👇 new code
            services.AddHttpClient("ServerAPI",
                  client => client.BaseAddress = uri)
                .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

            services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>()
              .CreateClient("ServerAPI"));
            
        }
    }

}
