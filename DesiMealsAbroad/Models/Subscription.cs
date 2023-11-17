using System;
namespace DesiMealsAbroad.Models;
public class Subscription
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int? NoOfMeals { get; set; }
    public string ImgUrl { get; set; }
    public decimal? Price { get; set; }
    public string Duration { get; set; }
    public string SubscriptionType { get; set; }
    public string Contents { get; set; }
    public string StripeProductId { get; set; }
}