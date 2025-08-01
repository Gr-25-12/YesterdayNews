using Microsoft.AspNetCore.Mvc;
using YesterdayNews.Services.IServices;

namespace YesterdayNews.Controllers
{
    public class SubscriptionController : Controller
    {
        private readonly ISubscriptionServices _subscriptionServices;

        public SubscriptionController(ISubscriptionServices subscriptionServices)
        {
            _subscriptionServices = subscriptionServices;
        }

        public IActionResult Index()
        {
            return View();
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

    }
}
