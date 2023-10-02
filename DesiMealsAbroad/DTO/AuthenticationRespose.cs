using System;
namespace DesiMealsAbroad.DTO
{
	public class AuthenticationRespose
	{	
			public string? PersonName { get; set; }
			public string? Email { get; set; }
			public DateTime Expiration { get; set; }
			public string? Token { get; set; }
    }
}

