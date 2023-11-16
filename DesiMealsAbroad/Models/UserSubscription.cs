using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DesiMealsAbroad.Models;

public class UserSubscription
{
    [Key]
    [Column("user_subscription_id")]
    public Guid UserSubscriptionId { get; set; }

    [Column("status")]
    [MaxLength(255)]
    public string Status { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("subscription_id")]
    [MaxLength(255)]
    public string SubscriptionId { get; set; }
}
