using YesterdayNews.Models.Db;

namespace YesterdayNews.Services.IServices
{
    public interface ICategoryService
    {
        List<Category> GetAll();
        Category GetOne(int id);
        void CreateCategory(Category newCategory);
        void EditCategory(Category updatedCategory);
        void Delete(int id);

    }
}
