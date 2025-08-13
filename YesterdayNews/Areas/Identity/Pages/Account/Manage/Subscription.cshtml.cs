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

        public SubscriptionModel(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        //public async Task<IActionResult> OnGetAsync()
        //{
        //    var user = await _userManager.GetUserAsync(User);
        //    if (user == null)
        //    {
        //        return NotFound("User not found.");
        //    }

        //    var subscription = await _context.Subscriptions
        //        .Include(s => s.SubscriptionType)
        //        .FirstOrDefaultAsync(s => s.UserId == user.Id);

        //    CurrentSubscription = subscription;

        //    return Page();
        //}

        public List<Subscription> SubscriptionHistory { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Current active subscription
            CurrentSubscription = await _context.Subscriptions
                .Include(s => s.SubscriptionType)
                .FirstOrDefaultAsync(s => s.UserId == user.Id &&
                                          s.Expires >= DateTime.UtcNow &&
                                          !s.IsDeleted);

            // All subscription history
            SubscriptionHistory = await _context.Subscriptions
                .Include(s => s.SubscriptionType)
                .Where(s => s.UserId == user.Id)
                .OrderByDescending(s => s.Created)
                .ToListAsync();

            return Page();
        }

    }
}