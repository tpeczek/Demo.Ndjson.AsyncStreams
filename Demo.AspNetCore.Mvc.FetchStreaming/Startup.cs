using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Demo.AspNetCore.Mvc.FetchStreaming.NdjsonStream;

namespace Demo.AspNetCore.Mvc.FetchStreaming
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .AddNdjsonStreamResult()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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
                .UseMvc()
                .Run(async (context) =>
                {
                    await context.Response.WriteAsync("-- Demo.AspNetCore.Mvc.FetchStreaming --");
                });
        }
    }
}
