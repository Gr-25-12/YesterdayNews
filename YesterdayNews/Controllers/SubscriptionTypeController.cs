using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using YesterdayNews.Models.Db;
using YesterdayNews.Services;
using YesterdayNews.Services.IServices;

namespace YesterdayNews.Controllers
{
    public class SubscriptionTypeController : Controller
    {
        private readonly ISubscriptionTypeServices _subscriptionTypeServices;

        public SubscriptionTypeController(ISubscriptionTypeServices subscriptionTypeServices)
        {
            _subscriptionTypeServices = subscriptionTypeServices;
        }
        public IActionResult Index()
        {
            
            return View();
            
        }



        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            var typesList = _subscriptionTypeServices.GetAll();
            return Json(new { data = typesList });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var typeToBeDeleted = _subscriptionTypeServices.GetOne(id);
            if (typeToBeDeleted == null)
            {
                return Json(new { success = false, message = "Subscription Type not found!" });
            }

         
                _subscriptionTypeServices.Delete(id);
                return Json(new { success = true, message = "Deleted successfully!" });
            
            
        }

        #endregion

    }
}
