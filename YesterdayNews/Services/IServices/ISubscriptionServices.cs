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
        /// <returns>List of all Ordered by DateCreation Descending</returns>
        public List<Subscription> GetAllByCreated();
        /// <summary>
        /// Get a list if all subscriptions including User and SubscriptionType
        /// </summary>
        /// <returns>List of all Ordered by Expiration Date Descending</returns>
        public List<Subscription> GetAllByExpires();
        public Subscription GetOne(int id);
        /// <summary>
        /// Get a subscription including User and SubscriptionType
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Subscription GetOneWithUserAndSubscriptionType(int id);
        void Add(Subscription newSubscription);
        void Edit(Subscription updatedSubscription);
        /// <summary>
        /// Don't use Delete() if not absolutely have to. use Cancel() instead.
        /// </summary>
        /// <param name="id"></param>
        void Delete(int id);
        /// <summary>
        /// Sets the propertie IsDeleted to true
        /// </summary>
        /// <param name="id"></param>
        void Cancel(int id);
    }
}
