﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RetroGamingWebAPI.HealthChecks;

namespace RetroGamingWebAPI
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
            string key = Configuration["ApplicationInsights:InstrumentationKey"];

            //services.AddHealthChecksUI();
            services
                .AddHealthChecks()
                .AddApplicationInsightsPublisher(key)
                .AddPrometheusGatewayPublisher("http://pushgateway:9091/metrics", "pushgateway")
                .AddCheck<RandomHealthCheck>("random", failureStatus: HealthStatus.Degraded);
               // .AddCheck<SqlServerHealthCheck>("sql");

            services.AddSingleton<SqlServerHealthCheck>(new SqlServerHealthCheck(
                new SqlConnection(Configuration.GetConnectionString("Test"))));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            HealthCheckOptions options = new HealthCheckOptions();
            options.ResultStatusCodes[HealthStatus.Unhealthy] = 418;
            options.AllowCachingResponses = true;
            options.Predicate = _ => true;
            options.ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse;

            app.UseHealthChecks("/health", options);
            //app.UseHealthChecksUI();
            //app.UseHealthChecksUI(setup =>
            //{
            //    setup.UIPath = "/show-health-ui"; // this is ui path in your browser
            //    setup.ApiPath = "/health-ui-api"; // the UI ( spa app )  use this path to get information from the store ( this is NOT the healthz path, is internal ui api )
            //});
            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
