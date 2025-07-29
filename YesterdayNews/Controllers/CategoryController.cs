using Microsoft.AspNetCore.Mvc;
using YesterdayNews.Models.Db;
using YesterdayNews.Services;
using YesterdayNews.Services.IServices;

namespace YesterdayNews.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public IActionResult Index()
        {
            List<Category> categories = _categoryService.GetAll();
            return View(categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

      

        [HttpPost]
        public IActionResult Create(Category newCategory )
        {
            if (ModelState.IsValid)
            {
                _categoryService.CreateCategory(newCategory);
                return RedirectToAction("Index");

            }

            return View(newCategory);
        }

        [HttpGet]
        public IActionResult Edit(int id) 
        {
            var category =_categoryService.GetOne(id);
            if (category == null) 
                return NotFound();

            return View(category);
        }
        [HttpPost]
        public IActionResult Edit(Category updatedCategory)
        {

            if (ModelState.IsValid)
            {
                _categoryService.EditCategory(updatedCategory);
                return RedirectToAction("Index");

            }

            return View(updatedCategory);

        }



        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                var categories = _categoryService
                    .GetAll()
                    .Select(c => new
                    {
                        id = c.Id,
                        name = c.Name
                    }).ToList();

                return Json(new
                {
                    success = true,
                    data = categories
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }



        [HttpDelete]
        public IActionResult Delete(int id)
        {
            try
            {
                var categoryToDelete = _categoryService.GetOne(id);
                if (categoryToDelete == null)
                {
                    TempData["error"] = "Category not found!";
                    return Json(new { success = false, message = "Category not found!" });
                }

                _categoryService.Delete(id);
                TempData["success"] = "Article deleted successfully!";
                return Json(new { success = true, message = "Deleted successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error while deleting: {ex.Message}"
                });
            }
        }
        #endregion





    }
}
