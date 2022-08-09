using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Admin.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }
        //private static async Task Run(WebAssemblyHostBuilder builder, string[] args)
        //{
        //    var apiUrl = "http://localhost:5001/api";

        //    builder.Services.AddRefitClient<CustomerApi>()
        //        .ConfigureHttpClient(c => { c.BaseAddress = new Uri(apiUrl); });

        //    // other code...
        //}

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();

                });
    }
}
