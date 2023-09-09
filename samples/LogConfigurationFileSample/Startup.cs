using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.AspNetCore;
using Serilog.Formatting.Compact;

namespace LogConfigurationFileSample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddOpenApiDocument();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var dedicatedOptionalLogger = new LoggerConfiguration()
                .SetSerilogPlusDefaultConfiguration()
                .WriteTo.File(new CompactJsonFormatter(),"App_Data/Logs/log_requests.json")
                .CreateLogger();

            app.UseSerilogPlusRequestLogging(p =>
            {
                p.LogMode = LogMode.LogAll;
                p.RequestHeaderLogMode = LogMode.LogAll;
                p.RequestBodyLogMode = LogMode.LogAll;
                p.RequestBodyLogTextLengthLimit = 5000;
                p.ResponseHeaderLogMode = LogMode.LogFailures;
                p.ResponseBodyLogMode = LogMode.LogAll;
                p.ResponseBodyLogTextLengthLimit = 5000;
                p.MaskFormat = "*****"; 
                p.MaskedProperties.Clear();
                p.MaskedProperties.Add("*password*");
                p.MaskedProperties.Add("*token*");
                p.Logger = dedicatedOptionalLogger; //if sets to null, request logger will use default global Serilog.Log.Logger
            });
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseOpenApi();
            app.UseSwaggerUi3();

            

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}