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
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using OpenIddict;
using AspNet.Security.OpenIdConnect.Primitives;
using Core.Test;

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
            // Add framework services.
            services.AddMvc()
                    .AddJsonOptions(options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
            services.AddDbContext<CoreContext>(options => options.UseMySql(Configuration["Data:MySQLDBConnectionString"]));
            services.AddScoped<IOrgRepository, OrgRepository>();
            services.AddScoped<IPortfolioRepository, PortfolioRepository>();
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<IMetricRepository, MetricRepository>();
            services.AddSingleton<IConfiguration>(Configuration);


            #region Identity Setup
            services.AddDbContext<CoreAuthContext>(options =>
            {
                options.UseMySql(Configuration["Data:MySQLDBConnectionString"]);
                options.UseOpenIddict();
            });
            services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<CoreAuthContext>()
            .AddDefaultTokenProviders();
            services.Configure<IdentityOptions>(options =>
            {
                // Password settings
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 2;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 100;

				//options.User.RequireUniqueEmail = true;
				options.ClaimsIdentity.UserNameClaimType = OpenIdConnectConstants.Claims.Name;
				options.ClaimsIdentity.UserIdClaimType = OpenIdConnectConstants.Claims.Subject;
				options.ClaimsIdentity.RoleClaimType = OpenIdConnectConstants.Claims.Role;
            });
            services.AddOpenIddict(options=>{
                options.AddEntityFrameworkCoreStores<CoreAuthContext>();
                options.AddMvcBinders();
                //options.EnableTokenEndpoint("/api/authtoken");
                options.EnableTokenEndpoint("/connect/token");
                options.AllowPasswordFlow();
                options.DisableHttpsRequirement();
				
            });
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseOAuthValidation();
            app.UseOpenIddict();
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            app.UseMiddleware<Core.Tools.ErrorHandlingMiddleware>();
            app.UseMvc();
			//app.UseMvcWithDefaultRoute();
			if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "TestWithDBWipe"){
				DataInitializer initer = new DataInitializer(app.ApplicationServices);
				initer.DeleteAllData();
				initer.CreateOrgStructure();
			}
        }
    }
}
