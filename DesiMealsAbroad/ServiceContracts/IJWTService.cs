
using DesiMealsAbroad.DTO;
namespace DesiMealsAbroad.ServiceContracts
{
	public interface IJWTService
	{
        AuthenticationRespose createToken(ApplicationUser user);
	}
}

