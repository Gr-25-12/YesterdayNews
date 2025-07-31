using YesterdayNews.Models.Db;

namespace YesterdayNews.Services.IServices
{
    public interface ISubscriptionServices
    {
        List<Subscription> GetAll();
    }
}
