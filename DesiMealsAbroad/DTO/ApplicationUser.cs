﻿using System;
namespace DesiMealsAbroad.DTO
{
	public class ApplicationUser
	{
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Password { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? StripeCustomerId { get; set; }
    }
}

