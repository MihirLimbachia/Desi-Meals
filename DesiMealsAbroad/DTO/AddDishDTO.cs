using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DesiMealsAbroad.DTO
{
	public class AddDishDTO
	{
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

    }
}

