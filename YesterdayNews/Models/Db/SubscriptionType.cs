using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YesterdayNews.Models.Db
{
    public class SubscriptionType
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string TypeName { get; set; }

        [Required]
        [StringLength(255)]
        public string Description { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();

    }
}
