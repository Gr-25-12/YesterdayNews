using Microsoft.EntityFrameworkCore;
using YesterdayNews.Data;
using YesterdayNews.Models.Db;
using YesterdayNews.Services.IServices;

namespace YesterdayNews.Services
{
    public class SubscriptionServices : ISubscriptionServices
    {
        private readonly ApplicationDbContext _db;
        public SubscriptionServices(ApplicationDbContext db)
        {
            _db = db;
        }
        public List<Subscription> GetAll()
        {
            return _db.Subscriptions.Include(u => u.User)
                                    .Include(st => st.SubscriptionType)
                                    .ToList();
        }
        public List<Subscription> GetAllByCreated()
        {
            return _db.Subscriptions.Include(u => u.User)
                                    .Include(st => st.SubscriptionType)
                                    .OrderByDescending(a => a.Created)
                                    .ToList();
        }
        public List<Subscription> GetAllByExpires()
        {
            return _db.Subscriptions.Include(u => u.User)
                                    .Include(st => st.SubscriptionType)
                                    .OrderByDescending(a => a.Expires)
                                    .ToList();
        }

        public Subscription GetOne(int id)
        {
            var subscription = _db.Subscriptions.Include(u => u.User)
                                                .Include(st => st.SubscriptionType)
                                                .FirstOrDefault(s => s.Id == id);
            return subscription;
        }
        public void Add(Subscription newSubscription)
        {
            _db.Subscriptions.Add(newSubscription);
            _db.SaveChanges();
        }
        public void Edit(Subscription updatedSubscription)
        {
            var subscription = _db.Subscriptions.FirstOrDefault(s => s.Id == updatedSubscription.Id);
            if (subscription == null) throw new Exception("Subscription not found");

            subscription.Created = updatedSubscription.Created;
            subscription.Expires = updatedSubscription.Expires;
            subscription.PaymentComplete = updatedSubscription.PaymentComplete;
            subscription.IsDeleted = updatedSubscription.IsDeleted;
            subscription.UserId = updatedSubscription.UserId;
            subscription.SubscriptionTypeId = updatedSubscription.SubscriptionTypeId;
            _db.SaveChanges();
        }
        public void Cancel(int id)
        {
            var subscription = _db.Subscriptions.FirstOrDefault(s => s.Id == id);
            if (subscription == null) throw new Exception("Subscription not found");

            subscription.IsDeleted = true;
            _db.SaveChanges();
        }

        //public void Delete(int id)
        //{
        //    var subscription = _db.Subscriptions.FirstOrDefault(s => s.Id == id);
        //    if (subscription == null) throw new Exception("Subscription not found");
        //    _db.Remove(subscription);
        //    _db.SaveChanges();
        //}
    }
}
