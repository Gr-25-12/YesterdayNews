using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YesterdayNews.Data;
using YesterdayNews.Models.Db;

namespace YesterdayNews.Controllers

{
    public class ArticlesController : Controller
    {
        private  readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public ArticlesController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Journalist,Editor")]
        public async Task<IActionResult> MyArticles(string sortOrder, string searchString)
        {
            var userId = _userManager.GetUserId(User);
            var articles = _context.Articles
                .Include(a => a.Category)
                .Where(a => a.AuthorId == userId);

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                articles = articles.Where(a =>
                    a.Headline.Contains(searchString) ||
                    a.ContentSummary.Contains(searchString));
            }

            articles = sortOrder switch
            {
                "date" => articles.OrderByDescending(a => a.DateStamp),
                "status" => articles.OrderBy(a => a.ArticleStatus),
                _ => articles.OrderBy(a => a.Headline)
            };

            return View(await articles.ToListAsync());
                       
        }

        // GET: Articles/Edit/5
        [Authorize(Roles = "Journalist,Editor")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var article = await _context.Articles.FindAsync(id);
            if (article == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (article.AuthorId != userId)
                return Forbid();

            if (article.ArticleStatus != ArticleStatus.Draft &&
                article.ArticleStatus != ArticleStatus.Published)
            {
                return BadRequest("Only draft or published articles can be edited.");
            }

            return View(article);
        }

        // POST: Articles/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Journalist,Editor")]
        public async Task<IActionResult> Edit(int id, Article updatedArticle)

        {
            if (id != updatedArticle.Id) return NotFound();

            var original = await _context.Articles.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
            if (original == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (original.AuthorId != userId) return Forbid();

            // If article was Published, set status to PendingReview
            if (original.ArticleStatus == ArticleStatus.Published)
                updatedArticle.ArticleStatus = ArticleStatus.PendingReview;
            else
                updatedArticle.ArticleStatus = original.ArticleStatus;

            // Keep original values
            updatedArticle.AuthorId = original.AuthorId;
            updatedArticle.DateStamp = original.DateStamp;

            if (ModelState.IsValid)
            {
                _context.Update(updatedArticle);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(MyArticles));
            }

            return View(updatedArticle);
        }



    }
}
