using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using YesterdayNews.Models.Db;
using YesterdayNews.Services;
using YesterdayNews.Services.IServices;
using YesterdayNews.Utils;

namespace YesterdayNews.Controllers
{
    [Authorize(Roles = StaticConsts.Role_Admin)]

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
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(SubscriptionType subscriptionType)
        {
            if (ModelState.IsValid)
            {
                _subscriptionTypeServices.Add(subscriptionType);
            TempData["Success"] = "Subscription Type created successfully!";
                return RedirectToAction("Index");
            }
            return View();
        }



        [HttpGet]
        public IActionResult Edit(int id)
        {
            var subscriptionType = _subscriptionTypeServices.GetOne(id);
            if (subscriptionType == null)
                return NotFound();

            return View(subscriptionType);
        }
        [HttpPost]
        public IActionResult Edit(SubscriptionType updatedSubscriptionType)
        {

            if (ModelState.IsValid)
            {
            TempData["Success"] = "Subscription Type updated successfully!";
                _subscriptionTypeServices.EditSubscriptionType(updatedSubscriptionType);
                return RedirectToAction("Index");

            }

            return View(updatedSubscriptionType);

        }





        #region API CALLS

        [HttpGet]
        [AllowAnonymous]
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
