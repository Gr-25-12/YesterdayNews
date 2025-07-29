using YesterdayNews.Data;
using YesterdayNews.Models.Db;
using YesterdayNews.Services.IServices;

namespace YesterdayNews.Services
{
    public class SubscriptionTypeServices : ISubscriptionTypeServices
    {
        private readonly ApplicationDbContext _db;
        public SubscriptionTypeServices(ApplicationDbContext db)
        {
            _db = db;
        }
        public List<SubscriptionType> GetAll()
        {
            return _db.SubscriptionTypes.ToList();
        }

        public SubscriptionType GetOne(int id)
        {
            var SubscriptionList = _db.SubscriptionTypes.FirstOrDefault(m => m.Id == id);
            return SubscriptionList;
        }

    }
}
