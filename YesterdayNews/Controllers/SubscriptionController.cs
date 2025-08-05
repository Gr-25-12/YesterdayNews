using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using YesterdayNews.Models.Db;
using YesterdayNews.Services;
using YesterdayNews.Services.IServices;

namespace YesterdayNews.Controllers
{
    public class SubscriptionController : Controller
    {
        private readonly ISubscriptionServices _subscriptionServices;
        private readonly ISubscriptionTypeServices _subscriptionTypeServices;
        private readonly UserManager<IdentityUser> _userManager;
        public SubscriptionController(ISubscriptionServices subscriptionServices, ISubscriptionTypeServices subscriptionTypeServices, UserManager<IdentityUser> userManager)
        {
            _subscriptionServices = subscriptionServices;
            _subscriptionTypeServices = subscriptionTypeServices;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Create()
        {
            var model = new Subscription
            {
                Created = DateTime.Today,
                UserId = null
            };

            var types = _subscriptionTypeServices.GetAll();
            ViewBag.SubscriptionTypes = types;
            return View(model);
        }

        [HttpPost]
        public IActionResult Create(Subscription subscription)
        {
            ModelState.Remove("User");
            ModelState.Remove("SubscriptionType");
            if (!ModelState.IsValid)
            {
                var types = _subscriptionTypeServices.GetAll();
                ViewBag.SubscriptionTypes = types;
                return View(subscription);
            }

            //Find and set all other to cancelled
            var activeSubs = _subscriptionServices.GetAll()
                      .Where(s => s.UserId == subscription.UserId && !s.IsDeleted);

            foreach (var sub in activeSubs)
            {
                _subscriptionServices.Cancel(sub.Id);
            }

            _subscriptionServices.Add(subscription);
            return RedirectToAction("Index");
        }
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var subscription = _subscriptionServices.GetOne(id);
            if (subscription == null)
                return NotFound();

            var types = _subscriptionTypeServices.GetAll();
            ViewBag.SubscriptionTypes = types;
            return View(subscription);
        }
        [HttpPost]
        public IActionResult Edit(Subscription subscription)
        {
            ModelState.Remove("User");
            ModelState.Remove("SubscriptionType");
            if (ModelState.IsValid)
            {
                _subscriptionServices.Edit(subscription);
                return RedirectToAction("Index");

            }
            var types = _subscriptionTypeServices.GetAll();
            ViewBag.SubscriptionTypes = types;
            return View(subscription);
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            var subscriptionsList = _subscriptionServices.GetAllByExpires()
                .Select(s => new
                {
                    s.Id,
                    UserEmail = s.User.Email,
                    s.Created,
                    s.Expires,
                    s.PaymentComplete,
                    s.IsDeleted,
                    TypeName = s.SubscriptionType.TypeName
                });

            return Json(new { data = subscriptionsList });
        }

        [HttpGet]
        public IActionResult Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return Json(new List<object>());

            var users = _userManager.Users
                .OfType<User>()
                .Where(u => u.FirstName.Contains(searchTerm) || u.LastName.Contains(searchTerm) || u.Email.Contains(searchTerm))
                .Select(u => new
                {
                    id = u.Id,
                    name = u.FirstName + " " + u.LastName
                })
                .Take(20)
                .ToList();

            return Json(users);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var subscriptionToBeDeleted = _subscriptionServices.GetOne(id);
            if (subscriptionToBeDeleted == null)
            {
                return Json(new { success = false, message = "Subscription not found!" });
            }


            _subscriptionServices.Cancel(id);
            return Json(new { success = true, message = "Subscription Deleted successfully!" });


        }

        #endregion

        [HttpGet]
        public IActionResult GetUserById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            var user = _userManager.Users
                .OfType<User>()
                .Where(u => u.Id == id)
                .Select(u => new
                {
                    firstName = u.FirstName,
                    lastName = u.LastName,
                    email = u.Email
                })
                .FirstOrDefault();

            if (user == null)
                return NotFound();

            return Json(user);
        }
    }
}




