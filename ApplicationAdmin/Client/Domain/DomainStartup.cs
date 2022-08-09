using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Refit;
using Microsoft.Extensions.DependencyInjection;
using Admin.Client.Api;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Public.Admin.Domain;

namespace Admin.Client.Services
{

    public static class DomainStartup
    {
        public static void InitDomain(this IServiceCollection services, WebAssemblyHostBuilder builder)
        {
            services.AddScoped<IAdminService, AdminService>();

            Uri uri = null;
            var configuration = builder.Configuration;

            //var ClientApiUrl = configuration["AdminApiUrl"];
            var ClientApiUrl = AppEnvironment.AdminApiUrl;

            uri = new Uri(ClientApiUrl);

            services.AddRefitClient<IAdminApi>()
                    .ConfigureHttpClient(c => c.BaseAddress = uri)
                    .SetHandlerLifetime(TimeSpan.FromMinutes(2));
        }
    }

}
