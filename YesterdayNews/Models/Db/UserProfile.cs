using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YesterdayNews.Models.Db
{
    public class UserProfile
    {
        [Key]
        public string UserId { get; set; }  // FK to IdentityUser

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DOB { get; set; }

        // Navigation (optional)
        [ForeignKey("UserId")]
        public IdentityUser User { get; set; }
    }
}
