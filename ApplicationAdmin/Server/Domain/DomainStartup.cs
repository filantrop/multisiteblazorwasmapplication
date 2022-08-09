using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Refit;
using Microsoft.Extensions.DependencyInjection;
using Admin.Server.Api;

namespace Admin.Server.Services
{
    
        public static class DomainStartup
        {
            public static void InitDomain(this IServiceCollection services)
            {
                services.AddScoped<IAdminService, AdminService>();
            
                services.AddRefitClient<IAdminApi>()
                        .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://api.exchangeratesapi.io/"))
                        .SetHandlerLifetime(TimeSpan.FromMinutes(2));
            }
        }
    
}
