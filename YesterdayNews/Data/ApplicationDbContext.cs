using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using YesterdayNews.Models.Db;

namespace YesterdayNews.Data;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; }
    public DbSet<SubscriptionType> SubscriptionTypes { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<Article> Articles { get; set; }
    public DbSet<User> Users { get; set; }
}
