using System.ComponentModel.DataAnnotations;

namespace YesterdayNews.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        [StringLength(255)]
        public string Description { get; set; }
    }
}
