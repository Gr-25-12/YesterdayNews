using FinanceServices.Data;
using FinanceServices.Services;
using FinanceServices.Services.BackgroundServices;
using FinanceServices.Services.IServices;
using FinanceServices.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Stripe;
using System.Threading.Tasks;
using YesterdayNews.Data;
using YesterdayNews.Hubs;
using YesterdayNews.Services;
using YesterdayNews.Services.IServices;
using YesterdayNews.Utils;
namespace YesterdayNews;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions => sqlOptions.EnableRetryOnFailure()));

        builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();
        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = $"/Identity/Account/Login";
            options.LogoutPath = $"/Identity/Account/Logout";
            options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
        });
        builder.Services.AddRazorPages();
        builder.Services.AddControllersWithViews();

        builder.Services.AddScoped<IArticleServices, ArticleServices>();
        builder.Services.AddScoped<IFileServices, FileServices>();
        builder.Services.AddTransient<EmailSender>();
        builder.Services.AddTransient<YesterdayNews.Utils.IEmailSender>(sp => sp.GetRequiredService<EmailSender>());
        builder.Services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender>(sp => sp.GetRequiredService<EmailSender>());
        builder.Services.AddScoped<ICategoryService, CategoryService>();
        builder.Services.AddScoped<ISubscriptionServices, SubscriptionServices>();
        builder.Services.AddScoped<ISubscriptionTypeServices, SubscriptionTypeServices>();
        builder.Services.AddScoped<ILikeService, LikeService>();
        builder.Services.AddScoped<IStripe, StripeServices>();
        builder.Services.AddScoped<IPdfService, PdfService>();
        builder.Services.AddScoped<IUserService, UserService>();

        builder.Services.AddScoped<IFinanceApiServices, FinanceApiServices>();
        builder.Services.AddScoped<IExternalNewsService, ExternalNewsService>();
        builder.Services.AddScoped<IDbInitalizlier, DbInitalizlier>();
        builder.Services.AddScoped<INewsChatService, NewsChatService>();
        builder.Services.AddHttpClient<ExternalNewsService>();
        builder.Services.AddHttpClient<DbInitalizlier>();
   
        builder.Services.AddHttpClient();
        builder.Services.AddAuthentication().AddGoogle(googleOptions =>
         {
             googleOptions.ClientId = builder.Configuration.GetSection("Google:ClientId").Get<string>()!;

             googleOptions.ClientSecret = builder.Configuration.GetSection("Google:ClientSecret").Get<string>()!;
         });
      

        builder.Services.AddAuthentication().AddMicrosoftAccount(microSoftOptions =>
        {
            microSoftOptions.ClientId = builder.Configuration.GetSection("Microsoft:ClientId").Get<string>()!;
            microSoftOptions.ClientSecret = builder.Configuration.GetSection("Microsoft:ClientSecret").Get<string>()!;

        });
        StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

        //Finance Services

        //register SignalR for the financeHub
        builder.Services.AddSignalR().AddJsonProtocol(options =>
        {
            options.PayloadSerializerOptions.PropertyNamingPolicy = null;
        });

        //Singletons
        builder.Services.AddSingleton<MarketDataCache>();
        builder.Services.AddSingleton<FinnhubApiCallsCounter>();
        builder.Services.AddSingleton<FinnhubApiService>();
        builder.Services.AddSingleton<FinnhubWebSocketService>();
        builder.Services.AddSingleton<CryptoSnapshotService>();
        builder.Services.AddSingleton<FinanceEventHandler>();
        //HostetService
        builder.Services.AddHostedService(provider => provider.GetRequiredService<FinnhubApiService>());
        builder.Services.AddHostedService(provider => provider.GetRequiredService<FinnhubWebSocketService>());
        builder.Services.AddHostedService(provider => provider.GetRequiredService<CryptoSnapshotService>());

        builder.Services.AddMemoryCache();



        builder.Services.AddQuartz(q =>
        {
            // Just use the name of your job that you created in the Jobs folder.
            var jobKey = new JobKey("DbInitalizlier");
            q.AddJob<DbInitalizlier>(opts => opts.WithIdentity(jobKey));

            q.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity("DbInitalizlier-trigger")
                //This Cron interval can be described as "run every 6 hours" (when second is zero)
                .WithCronSchedule("0 0 */6 * * ?")
            );
        });
        builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);


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

        //setting up the financeHub
        app.MapHub<FinanceHub>("/financeHub"); //endpoint clients will connect to this using JS

        var handler = app.Services.GetRequiredService<FinanceEventHandler>();
        var finnhubService = app.Services.GetRequiredService<FinnhubWebSocketService>();
        var finnhubApiService = app.Services.GetRequiredService<FinnhubApiService>();
        

        finnhubService.OnPriceUpdate += handler.HandlePriceUpdate;
        finnhubApiService.OnApiMarketStatusError += handler.HandleMarketStatusApiError;
        finnhubApiService.OnCachedUpdate += handler.HandleUpdateError;

        // in case we will run the seeding manually , iam keeping it for reference
        //var scope = app.Services.CreateScope();
        //{
        //    var dbIntalizer = scope.ServiceProvider.GetRequiredService<DbInitalizlier>();
        //    await dbIntalizer.SeedData();
        //}

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
        app.MapRazorPages();

        app.Run();

      
    }
}