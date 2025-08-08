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
        public void Delete(int id)
        {
            var type = _db.SubscriptionTypes.FirstOrDefault(m => m.Id == id);
            if (type == null)
                throw new Exception("SubscriptionType not found.");

            _db.SubscriptionTypes.Remove(type);
            _db.SaveChanges();
        }
        public void EditSubscriptionType(SubscriptionType updatedSubscriptionType)
        {
            var subscriptionType = _db.SubscriptionTypes.FirstOrDefault(m => m.Id == updatedSubscriptionType.Id);
            if (subscriptionType == null)
                throw new Exception("Subscription type not found.");

            subscriptionType.TypeName = updatedSubscriptionType.TypeName;
            subscriptionType.Price = updatedSubscriptionType.Price;
            subscriptionType.Description = updatedSubscriptionType.Description;
            _db.SaveChanges();
        }
     



        public void Add(SubscriptionType subscriptionType)
        {
            _db.SubscriptionTypes.Add(subscriptionType);
            _db.SaveChanges();
        }

    }
}
