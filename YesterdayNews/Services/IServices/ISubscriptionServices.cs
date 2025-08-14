using YesterdayNews.Models.Db;

namespace YesterdayNews.Services.IServices
{
    public interface ISubscriptionServices
    {
        /// <summary>
        /// Get a list if all subscriptions including User and SubscriptionType
        /// </summary>
        /// <returns>List of all unsorted</returns>
        List<Subscription> GetAll();
  
        /// <summary>
        /// Get a list if all subscriptions including User and SubscriptionType
        /// </summary>
        /// <returns>List of all Ordered by Expiration Date Descending</returns>
        public List<Subscription> GetAllByExpires();
        /// <summary>
        /// Get a subscription including User and SubscriptionType
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Subscription GetOne(int id);
        void Add(Subscription newSubscription);
        void Edit(Subscription updatedSubscription);
       
        /// <param name="id"></param>
        /// <summary>
        /// Sets the propertie IsDeleted to true
        /// </summary>
        /// <param name="id"></param>
        void Cancel(int id);
        public bool HasActiveSubscription(string userId);

        public Subscription getSelcetedPlan(int planId, string userId);


    }
}
