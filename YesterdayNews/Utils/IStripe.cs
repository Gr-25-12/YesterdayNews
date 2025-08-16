using Stripe;
using Stripe.Checkout;
using YesterdayNews.Models.Db;

namespace YesterdayNews.Utils
{
    public interface IStripe
    {
        public string? CreatePaymentSession(SubscriptionType plan, string domain, string userId, string email);
        public Session GetSession(string sessionId);
        public Refund RefundPayment(string paymentIntentId, long amountInCents);

    }
}
