using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Stripe;
using YesterdayNews.Data;
using YesterdayNews.Services;
using YesterdayNews.Services.IServices;
using YesterdayNews.Utils;
namespace YesterdayNews;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions => sqlOptions.EnableRetryOnFailure()));

        builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();
        builder.Services.ConfigureApplicationCookie(options => {
            options.LoginPath = $"/Identity/Account/Login";
            options.LogoutPath = $"/Identity/Account/Logout";
            options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
        });
        builder.Services.AddRazorPages();
        builder.Services.AddControllersWithViews();

        builder.Services.AddScoped<IArticleServices, ArticleServices>();
        builder.Services.AddScoped<IFileServices, FileServices>();
        builder.Services.AddScoped<IEmailSender, EmailSender>();
        builder.Services.AddScoped<ICategoryService, CategoryService>();
        builder.Services.AddScoped<ISubscriptionServices, SubscriptionServices>();
        builder.Services.AddScoped<ISubscriptionTypeServices, SubscriptionTypeServices>();
        builder.Services.AddScoped<ILikeService, LikeService>();
        builder.Services.AddScoped<IStripe, StripeServices>();


        builder.Services.AddAuthentication().AddGoogle(googleOptions =>
         {
             googleOptions.ClientId = builder.Configuration.GetSection("Google:ClientId").Get<string>()!;
             
             googleOptions.ClientSecret = builder.Configuration.GetSection("Google:ClientSecret").Get<string>()!;
         });
        builder.Services.AddAuthentication().AddFacebook(facebookOptions =>
        {
            facebookOptions.AppId = builder.Configuration.GetSection("Facebook:AppId").Get<string>()!;
            facebookOptions.AppSecret = builder.Configuration.GetSection("Facebook:AppSecret").Get<string>()!;
        });

        builder.Services.AddAuthentication().AddMicrosoftAccount(microSoftOptions =>
        {
            microSoftOptions.ClientId = builder.Configuration.GetSection("Microsoft:ClientId").Get<string>()!;
            microSoftOptions.ClientSecret = builder.Configuration.GetSection("Microsoft:ClientSecret").Get<string>()!;

        });
        StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];


        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
        app.MapRazorPages();

        app.Run();
    }
}