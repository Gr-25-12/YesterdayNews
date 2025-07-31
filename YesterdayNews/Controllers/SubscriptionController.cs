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
        public SubscriptionController(ISubscriptionServices subscriptionServices, ISubscriptionTypeServices subscriptionTypeServices) 
        {
            _subscriptionServices = subscriptionServices;
            _subscriptionTypeServices = subscriptionTypeServices;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Create()
        {
            var types = _subscriptionTypeServices.GetAll();
            ViewBag.SubscriptionTypeId = new SelectList(types, "Id", "Name");
            return View();
        }
    }
}
