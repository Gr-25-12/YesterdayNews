using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YesterdayNews.Models.Db
{
    public class User : IdentityUser
    {
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }

        [DataType(DataType.Date)]
        public DateOnly? DateOfBirth { get; set; } 

        // Navigation Properties
        public ICollection<Article> AuthoredArticles { get; set; } = new List<Article>();
        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();

        [NotMapped]
        public string Role { get; set; }

        [NotMapped] 
        public string FullName => $"{FirstName} {LastName}";

    }
}
