using All.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
//using Microsoft.OpenApi.Models;
using System;

namespace Public.Server
{

    public class Startup
    {
        private IWebHostEnvironment CurrentEnvironment { get; set; }
        //private IWebHostEnvironment CurrentEnvironment { get; set; }
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            CurrentEnvironment = env;
            //CurrentEnvironment = env;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {

            //var clientUrl = Configuration["ClientUrl"];
            var clientUrl = Environment.GetEnvironmentVariable(nameof(IEnvironmentItem.ClientUrl));
            var clientApiUrl = Environment.GetEnvironmentVariable(nameof(IEnvironmentItem.ClientApiUrl));

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins(clientUrl);
                });
            });


            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opt =>
                {
                    opt.Authority = clientUrl;
                    opt.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidAudience = clientApiUrl,
                        ValidIssuer = clientUrl
                    };
                    if (CurrentEnvironment.IsDevelopment())
                    {
                        opt.RequireHttpsMetadata = false;
                    }
                });

            services.AddControllersWithViews();
            services.AddRazorPages();

            //services.AddHttpLogging(logging =>
            //{
            //    logging.LoggingFields = HttpLoggingFields.All;
            //    logging.RequestHeaders.Add("sec-ch-ua");
            //    logging.ResponseHeaders.Add("MyResponseHeader");
            //    logging.MediaTypeOptions.AddText("application/javascript");
            //    logging.RequestBodyLogLimit = 4096;
            //    logging.ResponseBodyLogLimit = 4096;

            //});





        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });

            //app.UseHttpLogging();


        }
    }
}
