namespace DesiMealsAbroad.Models;
public class SubscriptionOrder
{
    public Guid Id { get; set; }
    public Guid UserSubscriptionId { get; set; }
    public string SubscriptionName { get; set; }
    public string SubscriptionDescription { get; set; }
    public int NoOfMeals { get; set; }
    public string ImgUrl { get; set; }
    public decimal Price { get; set; }
    public string SubscriptionType { get; set; }
    public DateTime CreateDate { get; set; }
}