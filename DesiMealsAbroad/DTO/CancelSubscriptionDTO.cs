using DesiMealsAbroad.Models;
namespace DesiMealsAbroad.DTO;
public class CancelSubscriptionDTO
{
    public string Email { get; set; }
    public UserSubscriptionDetails subscription { get; set; }
}



