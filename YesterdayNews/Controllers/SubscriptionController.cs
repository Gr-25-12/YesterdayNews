using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Stripe.Checkout;
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

        public bool HasActiveSubscription(string userId)
        {
            var sub =_subscriptionServices.HasActiveSubscription(userId);

            return sub != null;
        }

        //[Authorize(Roles = StaticConsts.Role_Customer)]
        //[HttpPost]
        //public IActionResult SubscribeNow(int planId)
        //{
        //    var claimsIdentity = (ClaimsIdentity)User.Identity;
        //    var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

        //    // Validate the plan exists
        //    var plan = _subscriptionServices.GetOne(planId);
        //    if (plan == null)
        //    {
        //        return NotFound();
        //    }

        //    // Check for existing subscription
        //    var currentSubscription = _subscriptionServices.getSelcetedPlan(planId, userId);



        //    // Stripe Checkout Session
        //    var domain = $"{Request.Scheme}://{Request.Host}/";



        //    var options = new Stripe.Checkout.SessionCreateOptions
        //    {

        //        LineItems = new List<SessionLineItemOptions>
        //        {
        //            new SessionLineItemOptions
        //            {
        //                Quantity = 2,
        //                PriceData= new SessionLineItemPriceDataOptions
        //                {
        //                    UnitAmount = (long?)(plan.SubscriptionType.Price * 100), 
        //                    Currency = "Kr",
        //                    ProductData = new SessionLineItemPriceDataProductDataOptions
        //                    {
        //                        Name = plan.SubscriptionType.TypeName,
        //                        Description = plan.SubscriptionType.Description,
        //                    },
        //                },
        //            },
        //        },
        //        Mode = "payment",
        //        SuccessUrl = domain + $"Subscription/PaymentConfirmation?session_id={currentSubscription.Id}",
        //        CancelUrl = domain + "Subscription",
        //        CustomerEmail = User.FindFirstValue(ClaimTypes.Email),

        //    };

        //    var service = new SessionService();
        //    var session = service.Create(options);

        //    Response.Headers.Add("Location", session.Url);
        //    return RedirectToAction(nameof(PaymentConfirmation), new { id = currentSubscription.Id });

        //}

        //[HttpGet]
        //public async Task<IActionResult> PaymentConfirmation(string session_id)
        //{
        //    var service = new SessionService();
        //    var session = await service.GetAsync(session_id);

        //    var existingSubscription = _subscriptionServices.GetAll()
        //        .FirstOrDefault(s => s.UserId == session.Metadata["user_id"] && s.SubscriptionTypeId == int.Parse(session.Metadata["plan_id"]) && !s.IsDeleted);

        //    // Verify payment was successful
        //    if (session.PaymentStatus == "paid")
        //    {
        //        var userId = session.Metadata["user_id"];
        //        var planId = session.Metadata["plan_id"];
        //        var numberOfDays = (existingSubscription.Expires - existingSubscription.Created)?.TotalDays ?? 0; // Fix: Use TotalDays to get a double value

        //        // Create subscription in your database
        //        var subscription = new Subscription
        //        {
        //            UserId = userId,
        //            SubscriptionTypeId = int.Parse(planId),
        //            Created = DateTime.UtcNow,
        //            Expires = DateTime.UtcNow.AddDays(numberOfDays), // Fix: AddDays expects a double
        //            PaymentComplete = true
        //        };

        //        _subscriptionServices.Add(subscription);

        //        return View("Success");
        //    }

        //    return View("PaymentFailed");
        //}

        //[Authorize(Roles = StaticConsts.Role_Customer)]
        //[HttpPost]
        //public IActionResult SubscribeNow(int planId)
        //{
        //    try
        //    {
        //        var claimsIdentity = (ClaimsIdentity)User.Identity;
        //        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

        //        // Validate the plan exists
        //        var plan = _subscriptionServices.GetOne(planId);
        //        if (plan == null)
        //        {
        //            TempData["Error"] = "Selected plan not found.";
        //            return RedirectToAction("Index", "Home");
        //        }

        //        // Check for existing active subscription
        //        if (_subscriptionServices.HasActiveSubscription(userId))
        //        {
        //            TempData["Info"] = "You already have an active subscription.";
        //            return RedirectToAction("Index", "Subscription");
        //        }

        //        // Create Stripe Checkout Session
        //        var domain = $"{Request.Scheme}://{Request.Host}/";

        //        var options = new SessionCreateOptions
        //        {
        //            PaymentMethodTypes = new List<string> { "card" },
        //            LineItems = new List<SessionLineItemOptions>
        //    {
        //        new SessionLineItemOptions
        //        {
        //            PriceData = new SessionLineItemPriceDataOptions
        //            {
        //                UnitAmount = (long)(plan.SubscriptionType.Price * 100),
        //                Currency = "sek",
        //                ProductData = new SessionLineItemPriceDataProductDataOptions
        //                {
        //                    Name = plan.SubscriptionType.TypeName,
        //                    Description = plan.SubscriptionType.Description,
        //                },
        //            },
        //            Quantity = 1,
        //        },
        //    },
        //            Mode = "payment", //  "payment" for one-time, "subscription" for recurring
        //            SuccessUrl = domain + $"Subscription/PaymentConfirmation?session_id={{CHECKOUT_SESSION_ID}}",
        //            CancelUrl = domain + "Subscription/PaymentCancelled",
        //            CustomerEmail = User.FindFirstValue(ClaimTypes.Email),
        //            Metadata = new Dictionary<string, string>
        //    {
        //        { "user_id", userId },
        //        { "plan_id", planId.ToString() }
        //    }
        //        };

        //        var service = new SessionService();
        //        var session = service.Create(options);

        //        return Redirect(session.Url);
        //    }
        //    catch (Exception ex)
        //    {
               
        //        TempData["Error"] = $"Unable to process payment. Please try again {ex.Message}.";
        //        return RedirectToAction("Index", "Home");
        //    }
        //}

        [Authorize(Roles = StaticConsts.Role_Customer)]
[HttpPost]
public async Task<IActionResult> SubscribeNow(int planId)
{
    try
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

        // Validate the plan exists
        var plan = _subscriptionTypeServices.GetOne(planId);
        if (plan == null)
        {
            return Json(new { success = false, message = "Selected plan not found." });
        }

        // Check for existing active subscription
        if (_subscriptionServices.HasActiveSubscription(userId))
        {
            return Json(new { success = false, message = "You already have an active subscription." });
        }

        // Create Stripe Checkout Session
        var domain = $"{Request.Scheme}://{Request.Host}/";

        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(plan.Price * 100),
                        Currency = "sek",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = plan.TypeName,
                            Description = plan.Description,
                        },
                    },
                    Quantity = 1,
                },
            },
            Mode = "payment",
            SuccessUrl = domain + $"Subscription/PaymentConfirmation?session_id={{CHECKOUT_SESSION_ID}}",
            CancelUrl = domain + "Subscription/PaymentCancelled",
            CustomerEmail = User.FindFirstValue(ClaimTypes.Email),
            Metadata = new Dictionary<string, string>
            {
                { "user_id", userId },
                { "plan_id", planId.ToString() }
            }
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options);

        return Json(new { success = true, redirectUrl = session.Url });
    }
    catch (Exception ex)
    {
        return Json(new { success = false, message = $"Unable to process payment. Please try again. Error: {ex.Message}" });
    }
}
        [HttpGet]
        public async Task<IActionResult> PaymentConfirmation(string session_id)
        {
            try
            {
                var service = new SessionService();
                var session = await service.GetAsync(session_id);

               
                if (session.PaymentStatus == "paid")
                {
                    var userId = session.Metadata["user_id"];
                    var planId = int.Parse(session.Metadata["plan_id"]);

                    // Check if subscription already exists (prevent duplicate)
                    var existingSubscription = _subscriptionServices.getSelcetedPlan(planId, userId);
                    if (existingSubscription != null && existingSubscription.PaymentComplete)
                    {
                        TempData["Info"] = "Subscription already activated.";
                        return View("Success");
                    }

                    
                    var subscription = new Subscription
                    {
                        UserId = userId,
                        SubscriptionTypeId = planId,
                        Created = DateTime.UtcNow,
                        Expires = DateTime.UtcNow.AddMonths(1), 
                        PaymentComplete = true
                    };

                    _subscriptionServices.Add(subscription);

                    TempData["Success"] = "Subscription activated successfully!";
                    return View("Success");
                }
                else
                {
                    TempData["Error"] = "Payment was not completed.";
                    return View("PaymentFailed");
                }
            }
            catch (Exception ex)
            {
                
                TempData["Error"] = $"Unable to confirm payment{ex}.";
                return View("PaymentFailed");
            }
        }

        [HttpGet]
        public IActionResult PaymentCancelled()
        {
            TempData["Info"] = "Payment was cancelled.";
            return RedirectToAction("Index", "Home");
        }
    }
}




