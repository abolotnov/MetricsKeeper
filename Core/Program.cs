using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Core.Test;

namespace Core
{
    public class Program
    {
        public static void Main(string[] args)
        {
          /*  OLD - core1.1
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();
                */

            //You can create a configuration that sets this environment variable, this way the database cleanup and all will happen
            //if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "TestWithDBWipe"){
			//	DataLoader loader = new DataLoader();
			//	loader.LoadData();    
			//}
			

           // host.Run();

            //New - 2.0


            BuildWebHost(args).Run();
          


        }
        public static IWebHost BuildWebHost(string[] args) =>
           WebHost.CreateDefaultBuilder(args)
               .UseStartup<Startup>()
               .Build();
    }
}
