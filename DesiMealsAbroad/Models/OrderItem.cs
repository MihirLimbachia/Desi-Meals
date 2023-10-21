using System;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DesiMealsAbroad.Models;

public class OrderItem
{
    public Guid OrderId { get; set; }
    public int ItemId { get; set; }
    public int Quantity { get; set; }

    [Required]
    [MaxLength(255)]
    public string Name { get; set; }

    [Required]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Price { get; set; }

    [Required]
    [MaxLength(10000)]
    public string ImgUrl { get; set; }

}
