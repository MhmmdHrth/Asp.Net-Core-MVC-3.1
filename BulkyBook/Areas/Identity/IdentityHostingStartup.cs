using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(BulkyBook.Areas.Identity.IdentityHostingStartup))]

namespace BulkyBook.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
            });
        }
    }
}