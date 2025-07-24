using YesterdayNews.Data;
using YesterdayNews.Models.Db;
using YesterdayNews.Services.IServices;

namespace YesterdayNews.Services
{
    public class SubscriptionServices : ISubscriptionServices
    {
        private readonly ApplicationDbContext _applicationDbContext;
        public SubscriptionServices (ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }
        public List <Subscription> GetAll()
        {
            return _applicationDbContext.Subscriptions.ToList();
        }
    }
}
