using Jaeger;
using JaegerWrapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTracing;
using OpenTracing.Util;

namespace ServiceA
{
    public class Startup
    {
        // Initializing the tracer for ServiceA
        private static readonly Tracer Tracer = Tracing.Init("ServiceA", new LoggerFactory());

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            GlobalTracer.Register(Tracer);
            services.AddOpenTracing();
            
            var serviceProvider = services.BuildServiceProvider();
            services.AddScoped<ITraceBuilder>(t => new TraceBuilder(serviceProvider.GetService<ITracer>()));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseMvc();
        }
    }
}