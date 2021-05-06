using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Demo.WeatherForecasts;

namespace Demo.Ndjson.AsyncStreams.AspNetCore.Mvc
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                .AddNdjson()
                .SetCompatibilityVersion(CompatibilityVersion.Latest);

            services.AddSingleton<IWeatherForecaster, WeatherForecaster>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            DefaultFilesOptions defaultFilesOptions = new DefaultFilesOptions();
            defaultFilesOptions.DefaultFileNames.Clear();
            defaultFilesOptions.DefaultFileNames.Add("fetch-streaming.html");

            app.UseDefaultFiles(defaultFilesOptions)
                .UseStaticFiles()
                .UseRouting()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
                })
                .Run(async (context) =>
                {
                    await context.Response.WriteAsync("-- Demo.Ndjson.AsyncStreams.AspNetCore.Mvc --");
                });
        }
    }
}
