using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using YesterdayNews.Models.Db;
using YesterdayNews.Services;
using YesterdayNews.Services.IServices;
using YesterdayNews.Utils;

namespace YesterdayNews.Controllers
{
    //[Authorize(Roles = StaticConsts.Role_Admin)]

    public class SubscriptionController : Controller
    {
        private readonly ISubscriptionServices _subscriptionServices;
        private readonly ISubscriptionTypeServices _subscriptionTypeServices;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IStripe _stripe;
        public SubscriptionController(ISubscriptionServices subscriptionServices, ISubscriptionTypeServices subscriptionTypeServices, UserManager<IdentityUser> userManager, IStripe stripe)
        {
            _subscriptionServices = subscriptionServices;
            _subscriptionTypeServices = subscriptionTypeServices;
            _userManager = userManager;
            _stripe = stripe;
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

        public bool HasActiveSubscription(string userId)
        {
            var sub = _subscriptionServices.HasActiveSubscription(userId);

            return sub != null;
        }
        [HttpPost]
        public IActionResult SubscribeNow(int planId)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            var email = User.FindFirstValue(ClaimTypes.Email);

            var plan = _subscriptionTypeServices.GetOne(planId);
            if (plan == null)
            {
                return Json(new { success = false, message = "Selected plan not found." });
            }

            if (_subscriptionServices.HasActiveSubscription(userId))
            {
                return Json(new { success = false, message = "You already have an active subscription." });
            }

            var domain = $"{Request.Scheme}://{Request.Host}/";
            var redirectUrl = _stripe.CreatePaymentSession(plan, domain, userId, email);

            return Json(new { success = true, redirectUrl });
        }

        [HttpGet("subscriptions/success")]
        public IActionResult Success(string session_id)
        {
            var session = _stripe.GetSession(session_id);

            if (session.PaymentStatus == "paid")
            {
                var userId = session.Metadata["user_id"];
                var planId = int.Parse(session.Metadata["plan_id"]);

                if (_subscriptionServices.getSelcetedPlan(planId, userId) is { PaymentComplete: true })
                {

                    TempData["Info"] = "Subscription already activated.";
                    return View("Success");
                }

                var plan = _subscriptionTypeServices.GetOne(planId);
                DateTime expiresAt = plan.TypeName switch
                {
                    StaticConsts.SubscriptionType_Yearly => DateTime.UtcNow.AddYears(1),
                    StaticConsts.SubscriptionType_Quarterly => DateTime.UtcNow.AddMonths(3),
                    StaticConsts.SubscriptionType_Monthly => DateTime.UtcNow.AddMonths(1),
                    StaticConsts.SubscriptionType_Weekly => DateTime.UtcNow.AddDays(7),
                    _ => DateTime.UtcNow.AddDays(1)
                };


                var subscription = new Subscription
                {
                    UserId = userId,
                    SubscriptionTypeId = planId,
                    Created = DateTime.UtcNow,
                    Expires = expiresAt,
                    PaymentComplete = true
                };

                _subscriptionServices.Add(subscription);

                TempData["Success"] = "Subscription activated successfully!";
                // logic to send the emails 
                return View("Success");
            }

            TempData["Error"] = "Payment was not completed.";
            return View("PaymentFailed");
        }

    }
}

