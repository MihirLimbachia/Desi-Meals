namespace DesiMealsAbroad.DTO;
using global::DesiMealsAbroad.Models;

public class ReactivateSubscriptionDTO
{
    public string Email { get; set; }
    public UserSubscriptionDetails subscription { get; set; }
}
