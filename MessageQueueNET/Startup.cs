using MessageQueueNET.Extensions.DependencyInjection;
using MessageQueueNET.Middleware.Authentication;
using MessageQueueNET.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MessageQueueNET
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
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "MessageQueue.NET", Version = "v1" });
            });

            services.AddQueuesService();
            switch (Configuration["Persist:Type"]?.ToLower())
            {
                case "filesystem":
                    services.AddQueuePersitFileSystem(config =>
                        {
                          config.RootPath = Configuration["Persist:RootPath"];
                        });
                    break;
                default:
                    services.AddQueuePersitNone();
                    break;
            }
            
            services.AddRestorePersistedQueuesService();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, 
                              IWebHostEnvironment env,
                              RestorePersistedQueuesService _restoreQueues)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MessageQueue.NET v1"));
            }

            _restoreQueues.Restore().Wait();

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            switch(Configuration["Authentication:Type"]?.ToLower())
            {
                case "basic":
                    app.UseMiddleware<BasicAuthenticationMiddleware>();
                    break;
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
