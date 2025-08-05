using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YesterdayNews.Models.Db
{
    public enum ArticleStatus
    {
        Draft,
        PendingReview,
        Rejected,
        Published,
        Archived
    }
    public class Article
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public DateTime DateStamp { get; set; }
        [Required]
        [StringLength(255)]
        public string LinkText { get; set; }
        [Required]
        [StringLength(255)]
        public string Headline { get; set; }
        [Required]
        [StringLength(500)]
        [Display(Name = "Summary")]
        public string ContentSummary { get; set; }
        [Required]
        public string Content { get; set; }
        [Required]
        [Display(Name = "Author")]
        public string AuthorId { get; set; }  // Changed from List<User>

        [ForeignKey("AuthorId")]
        [ValidateNever]
        public User Author { get; set; }
        public int Views { get; set; }
        public int Likes { get; set; }
        [Required]
        [StringLength(500)]
        public string ImageLink { get; set; }
        [Required]
        public int CategoryId { get; set; }
        [Required]
        public ArticleStatus ArticleStatus { get; set; } = ArticleStatus.Draft;
        [ForeignKey("CategoryId")]
        [ValidateNever]
        public Category Category { get; set; }
        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        public ICollection<UserArticleLike> LikedByUsers { get; set; } = new List<UserArticleLike>();

        [NotMapped]
        public bool IsLikedByCurrentUser { get; set; }

        
        public string? RejectionReason { get; set; }
    }
}
