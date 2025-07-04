using Hangfire;
using Hangfire.MemoryStorage;
using HoroScope.DAL;
using HoroScope.Interfaces;
using HoroScope.Models;
using HoroScope.Services;
using HoroScope.Services.Implementations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace HoroScope
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddControllersWithViews();

            builder.Services.AddIdentity<AppUser, IdentityRole>(opt =>
            {
                opt.Password.RequiredLength = 8;
                opt.Password.RequireUppercase = false;
                opt.Password.RequireNonAlphanumeric = false;

                opt.User.RequireUniqueEmail = true;

                opt.Lockout.MaxFailedAccessAttempts = 3;
                opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
                opt.Lockout.AllowedForNewUsers = true;

                opt.SignIn.RequireConfirmedEmail = true;

            }).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();


            builder.Services.AddDbContext<AppDbContext>(opt =>
            {
                opt.UseSqlServer(builder.Configuration.GetConnectionString("default"));
            });
            builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));
            builder.Services.ConfigureApplicationCookie(opt =>
            {
                opt.AccessDeniedPath = "/Account/AccessDenied";
            });

            builder.Services.AddHttpClient<AstroService>(client =>
            {
                client.BaseAddress = new Uri("http://127.0.0.1:5000/");
            });

            builder.Services.AddHttpClient<NominatimService>();
            builder.Services.AddScoped<ILayoutService, LayoutService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IBasketService, BasketService>();
            builder.Services.AddScoped<ISubscriptionReminderService, SubscriptionReminderService>();
            builder.Services.AddHangfire(x => x.UseMemoryStorage());
            builder.Services.AddHangfireServer();

            var app = builder.Build();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseStaticFiles();
            app.UseHangfireDashboard();

            RecurringJob.AddOrUpdate<ISubscriptionReminderService>(
                "reminder-job",
                service => service.SendExpiringRemindersAsync(),
                Cron.Daily);

            StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];


            app.MapControllerRoute(
                "default",
                "{area:exists}/{controller=Home}/{action=Index}/{id?}");


            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
