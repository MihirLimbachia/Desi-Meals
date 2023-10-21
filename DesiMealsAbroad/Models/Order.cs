using System;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DesiMealsAbroad.Models;

public class Order
{
    [Key]
    public Guid OrderId { get; set; }

    [MaxLength(255)]
    public string Email { get; set; }

    public DateTime OrderDate { get; set; }

    [Column(TypeName = "numeric(10,2)")]
    public decimal TotalAmount { get; set; }

    [MaxLength(20)]
    public string Status { get; set; }

    public List<OrderItem>? OrderItems { get; set; }
}


