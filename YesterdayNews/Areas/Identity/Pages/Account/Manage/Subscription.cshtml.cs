using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using YesterdayNews.Data;
using YesterdayNews.Models.Db;

namespace YesterdayNews.Areas.Identity.Pages.Account.Manage
{
    public class SubscriptionModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public Subscription? CurrentSubscription { get; set; }
        public List<Subscription> SubscriptionHistory { get; set; } = new();

        public SubscriptionModel(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Get all subscriptions for this user, ordered by creation date (most recent first)
            var allSubscriptions = await _context.Subscriptions
                .Include(s => s.SubscriptionType)
                .Where(s => s.UserId == user.Id )
                .OrderByDescending(s => s.Created)
                .ToListAsync();

            if (allSubscriptions.Any())
            {
                CurrentSubscription = allSubscriptions.First();

                
                SubscriptionHistory = allSubscriptions.Skip(1).ToList();
            }

            return Page();
        }

    }
}