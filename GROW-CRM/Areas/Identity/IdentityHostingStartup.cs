using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(GROW_CRM.Areas.Identity.IdentityHostingStartup))]
namespace GROW_CRM.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
            });
        }
    }
}