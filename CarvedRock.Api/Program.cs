using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace CarvedRock.Api
{
    public class Program
    {

        // public static void Main(string[] args)
        // {
        //     CreateHostBuilder(args).Build().Run();
        // }

        public static int Main(string[] args)
        {
            // Log.Logger = new LoggerConfiguration()
            //     .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            //     .Enrich.FromLogContext()
            //     .WriteTo.Console()
            //     .CreateLogger();

            var name = typeof(Program).Assembly.GetName().Name;

                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                    .Enrich.FromLogContext()
                    .Enrich.WithMachineName()
                    .Enrich.WithProperty("Assembly", name)
                    //.WriteTo.Seq(serverUrl: "http://host.docker.internal:5341")
                    .WriteTo.Seq(serverUrl: "http://seq_in_dc:5341")//deq_in_dc
                    .WriteTo.Console()
                    .CreateLogger();
            
            try
            {
                Log.Information("Starting web host");
                CreateHostBuilder(args).Build().Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
		    // http://bit.ly/aspnet-builder-defaults
            Host.CreateDefaultBuilder(args)
            .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
