using GROW_CRM.BackgroundTasks;
using GROW_CRM.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace GROW_CRM
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IHost host = CreateHostBuilder(args).Build();

            using( var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                try
                {

                    var identityContext = services.GetRequiredService<ApplicationDbContext>();
                    identityContext.Database.Migrate();
                    ApplicationSeedData.SeedAsync(identityContext, services).Wait();

                    var context = services.GetRequiredService<GROWContext>();
                    context.Database.Migrate();
                    GROWSeedData.Initialize(services);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred seeding the DB.");
                }
            }
                
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureServices(services =>
                {
                    services.AddHostedService<DeleteTempMembers>();
                    services.AddScoped<IScopedDeleteEmptyMembers, ScopedDeleteEmptyMembers>();
                    services.AddControllersWithViews()
                        .AddNewtonsoftJson(options =>
                                                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                                            );
                });
    }
}
