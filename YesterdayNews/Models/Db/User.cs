using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace YesterdayNews.Models.Db
{
    public class User : IdentityUser
    {
        [Required]
        [MaxLength(50)]
        [Display(Name ="First Name")]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name = "Last Name")]

        public string LastName { get; set; }

        [DataType(DataType.Date)]
        public DateOnly? DateOfBirth { get; set; } 

        // Navigation Properties
        public ICollection<Article> AuthoredArticles { get; set; } = new List<Article>();
        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();

    }
}
