using System;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DesiMealsAbroad.Models
{
    public class Meal
    {
        [Key]
        public int MealID { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(10000)]
        public string Description { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }

        [MaxLength(50)]
        public string Category { get; set; }

        [MaxLength(255)]
        public string ImageURL { get; set; }

        // Define a collection of dishes for the meal
        public List<Dish> Dishes { get; set; }
    }
}

