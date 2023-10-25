using System;
namespace DesiMealsAbroad.DTO;

public class UpdateCartItems
{
    public List<CartItemDTO> CartItems { get; set; }
    public string Email { get; set; }
}


