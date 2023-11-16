using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DesiMealsAbroad.Models;

public class UserSubscriptionDetails
{
    public Guid UserSubscriptionId { get; set; }

    public string Status { get; set; }


    public DateTime CreatedAt { get; set; }

    public string SubscriptionId { get; set; }

    public string Name { get; set; }
    
    public int? NoOfMeals { get; set; }

    public string ImgUrl { get; set; }

    public decimal? Price { get; set; }
    
    public string SubscriptionType { get; set; }

}

