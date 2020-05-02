using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Business.AspNet;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using Business.Core.Utils;
using Microsoft.AspNetCore.Http.Features;

namespace WebAPI
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("any", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            });

            services.AddMvc(option => option.EnableEndpointRouting = false).SetCompatibilityVersion(CompatibilityVersion.Latest);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            /*
            location / {
                proxy_pass http://github.com:80;
                proxy_set_header Host $host;
                proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
                proxy_set_header X-Forwarded-Proto $scheme;
            }
            */

            app.UseCors("any");

            app.CreateBusiness()
                .UseDoc(options =>
                {
                    options.Debug = true;
                    options.Benchmark = true;
                    options.Navigtion = true;
                    options.Testing = true;
                })
                .UseResultType(typeof(MyResultObject<>))//Use your ResultObject
                .UseWebSockets(options =>
                {
                    options.KeepAliveInterval = TimeSpan.FromSeconds(120);
                    options.ReceiveBufferSize = 4 * 1024;
                })
                .UseServer(server =>
                {
                    //kestrel
                    server.KestrelOptions.Limits.MinRequestBodyDataRate = null;
                    server.KestrelOptions.Limits.MinResponseDataRate = null;
                    server.KestrelOptions.Limits.MaxConcurrentConnections = long.MaxValue;
                    server.KestrelOptions.Limits.MaxConcurrentUpgradedConnections = long.MaxValue;
                    server.KestrelOptions.Limits.MaxRequestBodySize = null;
                    //form
                    server.FormOptions.KeyLengthLimit = int.MaxValue;
                    server.FormOptions.ValueCountLimit = int.MaxValue;
                    server.FormOptions.ValueLengthLimit = int.MaxValue;
                    server.FormOptions.MultipartHeadersLengthLimit = int.MaxValue;
                    server.FormOptions.MultipartBodyLengthLimit = long.MaxValue;
                    server.FormOptions.MultipartBoundaryLengthLimit = int.MaxValue;
                })
                .Build();
        }
    }
}
