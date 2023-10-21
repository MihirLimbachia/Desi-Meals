using System;
namespace DesiMealsAbroad.DTO
{
    public class GetOrdersDTO
    {
        public string Email { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

}

