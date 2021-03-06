using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stripe;
using System;

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

            services.Configure<StripeSettings>(Configuration.GetSection("Stripe"));

            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            services.AddRazorPages();

            services.AddCustomCookiesPath()
                    .AddCustomeAuthentication()
                    .AddCustomeSession();
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

            StripeConfiguration.ApiKey = Configuration.GetSection("Stripe")["SecretKey"];

            app.UseSession();
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

    internal static class CustomExtensionMethods
    {
        public static IServiceCollection AddCustomeDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<IdentityUser, IdentityRole>().AddDefaultTokenProviders()
                    .AddEntityFrameworkStores<ApplicationDbContext>();

            return services;
        }

        public static IServiceCollection AddCustomeAuthentication(this IServiceCollection services)
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

        public static IServiceCollection AddCustomCookiesPath(this IServiceCollection services)
        {
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = $"/Identity/Account/Login";
                options.LogoutPath = $"/Identity/Account/Logout";
                options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
            });

            return services;
        }

        public static IServiceCollection AddCustomeSession(this IServiceCollection services)
        {
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            return services;
        }
    }
}