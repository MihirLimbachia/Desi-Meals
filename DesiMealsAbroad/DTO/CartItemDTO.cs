using System;
using DesiMealsAbroad.Models;

namespace DesiMealsAbroad.DTO;

public class CartItemDTO
{
    public Dish Item { get; set; }
    public int Quantity { get; set; }
}

