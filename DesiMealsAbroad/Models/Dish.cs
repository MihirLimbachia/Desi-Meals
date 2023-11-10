using System;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DesiMealsAbroad.Models
{
    public class Dish
    {
        
        public int ID { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }

        [Required]
        [MaxLength(10000)]
        public string ImgUrl { get; set; }

        [Required]
        public List<string> Ingredients { get; set; }

        [Required]
        public decimal Calories { get; set; }

        [Required]
        [MaxLength(100)]
        public string Origin { get; set; }

        [Required]
        [MaxLength(100)]
        public string SpiceLevel { get; set; }

    }
}

