using Microsoft.EntityFrameworkCore;
using YesterdayNews.Data;
using YesterdayNews.Models.Db;
using YesterdayNews.Services.IServices;


namespace YesterdayNews.Services
{
    public class CategoryService : ICategoryService
    {

        private readonly ApplicationDbContext _db;

        public CategoryService(ApplicationDbContext db)
        {
            _db = db;
        }

        public void CreateCategory(Category newCategory)
        {
            _db.Categories.Add(newCategory);
            _db.SaveChanges();
        }

      

        

        public void EditCategory(Category updatedCategory)
        {
            var category = _db.Categories.FirstOrDefault(m => m.Id == updatedCategory.Id);
            if (category == null)
                throw new Exception("Category not found.");

            category.Name = updatedCategory.Name;
            _db.SaveChanges();
        }
        public void Delete(int id)
        {
            var category = _db.Categories.FirstOrDefault(m => m.Id == id);
            if (category == null)
                throw new Exception("Category not found.");

            _db.Categories.Remove(category);
            _db.SaveChanges();
        }


        public Category GetOne(int id)
        {
            var category = _db.Categories.FirstOrDefault(m => m.Id == id);
            return category;
        }

        public List<Category> GetAll()
        {
            return _db.Categories.ToList();

        }
    }
}
