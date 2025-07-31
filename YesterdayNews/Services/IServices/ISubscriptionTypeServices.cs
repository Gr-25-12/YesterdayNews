using YesterdayNews.Models.Db;

namespace YesterdayNews.Services.IServices
{
    public interface ISubscriptionTypeServices
    {
        List<SubscriptionType> GetAll();
        SubscriptionType GetOne(int id);

        void EditSubscriptionType(Category updatedSubscriptionType);
        void Delete(int id);
    }
}
