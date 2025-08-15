using Stripe;
using Stripe.Checkout;
using YesterdayNews.Models.Db;

namespace YesterdayNews.Utils
{
    public class StripeServices : IStripe
    {
        public StripeServices(IConfiguration configuration)
        {
            StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"];
        }

        public string? CreatePaymentSession(SubscriptionType plan, string domain, string userId, string email)
        {
            

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(plan.Price * 100),
                            Currency = "sek",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Yesterday News " + plan.TypeName + " Subscription 👍",
                                Description = plan.Description,
                              
                            },
                        },
                        Quantity = 1,
                    },
                },
                Mode = "payment",
                SuccessUrl = $"{domain}subscriptions/success?session_Id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{domain}identity/account/manage",
                CustomerEmail = email,
                Metadata = new Dictionary<string, string>
                {
                    { "user_id", userId },
                    { "plan_id", plan.Id.ToString() }
                }
            };

            var service = new SessionService();
            var session = service.Create(options);

            return session.Url;
        }

      
        public Session GetSession(string sessionId)
        {
            var service = new SessionService();
            return service.Get(sessionId);
        }

      
        public Refund RefundPayment(string paymentIntentId, long amountInOre)
        {
            var refundService = new RefundService();
            var refundOptions = new RefundCreateOptions
            {
                PaymentIntent = paymentIntentId,
                Amount = amountInOre
            };

            return refundService.Create(refundOptions);
        }
    }
}

