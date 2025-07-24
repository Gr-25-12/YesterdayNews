using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

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
        [StringLength(10)]
        public string Price { get; set; }

        //[ForeignKey("SubsciptionId")]

    }
}
