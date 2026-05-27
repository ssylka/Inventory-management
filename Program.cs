using Inventory_Managment.Models;
using Inventory_Managment.Services;
using AspNet.Security.OAuth.GitHub;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System;
using static System.Formats.Asn1.AsnWriter;
// import SalesforceService
//using SalesforceCore;

namespace Inventory_Managment
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews()
                    .AddViewLocalization()
                    .AddDataAnnotationsLocalization();

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

            builder.Services.AddScoped<CustomIdService>();
            builder.Services.AddScoped<InventoryService>();
            builder.Services.AddScoped<EmailService>();
            builder.Services.AddScoped<ImageService>();
            builder.Services.AddScoped<StatService>();

            builder.Services.AddHttpClient();
            //SalesforceService
            builder.Services.AddScoped<SalesforceService>();

            builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedEmail = true;
            })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.Configure<SecurityStampValidatorOptions>(options =>
            {
                options.ValidationInterval = TimeSpan.Zero;
                // Sometimes TimeSpan.Zero would query the DB on every request and exhaust the connection pool.
                // If it happen, uncomment a string below:
                // options.ValidationInterval = TimeSpan.FromSeconds(30);
            });

            builder.Services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = builder.Configuration["Auth:Google:ClientId"];
                    options.ClientSecret = builder.Configuration["Auth:Google:ClientSecret"];
                })
                .AddFacebook(options =>
                {
                    options.AppId = builder.Configuration["Auth:Facebook:AppId"];
                    options.AppSecret = builder.Configuration["Auth:Facebook:AppSecret"];
                })
                .AddGitHub(options =>
                {
                    options.ClientId = builder.Configuration["GitHub:ClientId"]!;
                    options.ClientSecret = builder.Configuration["GitHub:ClientSecret"]!;
                    options.Scope.Add("user:email");
                });

            builder.Services.AddLocalization(o => o.ResourcesPath = "");

            var app = builder.Build();

            var supportedCultures = new[] { "en", "ru" };
            var localizationOptions = new RequestLocalizationOptions()
                .SetDefaultCulture("en")
                .AddSupportedCultures(supportedCultures)
                .AddSupportedUICultures(supportedCultures);

            app.UseRequestLocalization(localizationOptions);

            app.UseForwardedHeaders(new ForwardedHeadersOptions //Nginx
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            using (var scope = app.Services.CreateScope())
            {   
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                foreach (var role in new[] { "Admin", "Active", "Blocked", "Unverified" })
                {
                    if (!await roleManager.RoleExistsAsync(role))
                        await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
            await app.RunAsync();
        }
    }
}
