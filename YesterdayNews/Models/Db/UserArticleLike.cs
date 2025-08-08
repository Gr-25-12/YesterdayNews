using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using YesterdayNews.Models.Db;

public class UserArticleLike
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; }

    [Required]
    public int ArticleId { get; set; }

    public DateTime LikedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("UserId")]
    [ValidateNever]
    public User User { get; set; }

    [ForeignKey("ArticleId")]
    [ValidateNever]
    public Article Article { get; set; }
}