﻿using System.ComponentModel.DataAnnotations;

namespace YesterdayNews.Models.Db
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }
       
    }
}
