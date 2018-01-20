using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Core.Context;
using Microsoft.EntityFrameworkCore;
using Core.Repository;
using Core.Data;
using Core.Test;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Core
{
    public class Startup
    {

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                    .AddJsonOptions(options => 
                                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
            services.AddDbContext<CoreContext>(options => options.UseMySql(Configuration["Data:MySQLDBConnectionString"]));
            services.AddScoped<IOrgRepository, OrgRepository>();
            services.AddScoped<IPortfolioRepository, PortfolioRepository>();
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<IMetricRepository, MetricRepository>();
            services.AddSingleton<IConfiguration>(Configuration);

            #region Swagger Configuration
            services.AddSwaggerGen(options =>
            {
                options.DescribeAllEnumsAsStrings();
                options.SwaggerDoc("v1", new Swashbuckle.AspNetCore.Swagger.Info
                {
                    Title = "Metrics Keeper Core API",
                    Version = "1.0",
                    TermsOfService = "Development purposes only, your data is not retained"
                });
            });
            #endregion

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            app.UseMiddleware<Core.Tools.ErrorHandlingMiddleware>();
            app.UseMvc();

            #region Swagger COnfiguration
            app.UseSwagger().UseSwaggerUI(c=>{
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Swagger yo!");
            });
#endregion

            //app.UseMvcWithDefaultRoute();
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "TestWithDBWipe")
            {
                DataInitializer initer = new DataInitializer(app.ApplicationServices);
                initer.DeleteAllData();
                initer.CreateOrgStructure();
            }
        }
    }
}
