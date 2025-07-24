using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YesterdayNews.Models.Db
{
    public class Subscription
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public DateTime Created { get; set; } = DateTime.UtcNow;

        public DateTime? Expires { get; set; }

        [Required]
        public bool PaymentComplete { get; set; } = false;

        [Required]
        public bool IsDeleted { get; set; } = false;



        [Required]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }


        [Required]
        public int SubscriptionTypeId { get; set; }
        [ForeignKey("SubscriptionTypeId")]
        public SubscriptionType SubscriptionType { get; set; }

    }
}