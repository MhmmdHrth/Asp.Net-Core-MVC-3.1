using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.DataAccess.Repository;
using BulkyBook.Utility;
using BulkyBook.Models;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace BulkyBook
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {

            services.AddCustomeDbContext(Configuration);

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddSingleton<IEmailSender, EmailSender>();

            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            services.AddRazorPages();

            services.AddCustomCookiesPath(Configuration)
                    .AddCustomeAuthentication(Configuration);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: SD.pattern);
                endpoints.MapRazorPages();
            });
        }
    }

    static class CustomExtensionMethods
    {
        public static IServiceCollection AddCustomeDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<IdentityUser, IdentityRole>().AddDefaultTokenProviders()
                    .AddEntityFrameworkStores<ApplicationDbContext>();

            return services;
        }

        public static IServiceCollection AddCustomeAuthentication(this IServiceCollection services, IConfiguration Configuration)
        {
            services.AddAuthentication().AddFacebook(options =>
            {
                options.AppId = "2691071014348881";
                options.AppSecret = "06e1ae0842d673efb2fce787f1f9c2d0";
            });

            services.AddAuthentication().AddGoogle(options =>
            {
                options.ClientId = "709466992362-hr6o1r3s7tblp8f4elrirdnsoqjrq1a9.apps.googleusercontent.com";
                options.ClientSecret = "wjplXrK1sAqFuJsEkcYcsEzR";
            });

            return services;
        }

        public static IServiceCollection AddCustomCookiesPath(this IServiceCollection services, IConfiguration Configuration)
        {
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = $"/Identity/Account/Login";
                options.LogoutPath = $"/Identity/Account/Logout";
                options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
            });

            return services;
        }
    }
}
