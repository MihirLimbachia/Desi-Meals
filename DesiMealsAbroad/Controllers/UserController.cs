using DesiMealsAbroad.ServiceContracts;
using DesiMealsAbroad.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace DesiMealsAbroad.Controllers;

[ApiController]
[AllowAnonymous]
[Route("/api/auth")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IJWTService _jwtService;
    private readonly UserRepository _userRepository;

    public UserController(ILogger<UserController> logger, IJWTService jwtService, UserRepository userRepository)
    {
        _logger = logger;
        _jwtService = jwtService;
        _userRepository = userRepository;
    }

    [HttpPost("register")]
    public IActionResult RegisterUser([FromBody] RegisterUserDTO user)
    {
        _userRepository.AddUser(user);

        // Generate a JWT token for the registered user

        return NoContent();
    }

    [HttpPost("login")]
    public IActionResult LoginUser([FromBody] LoginUserDTO userDTO)
    {
        ApplicationUser? user = _userRepository.GetUserByEmail(userDTO.Email);

        if (user == null)
        {
            
            return BadRequest("User not registered");
        }

        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(userDTO.Password, user.Password);

        if (isPasswordValid)
        {

            AuthenticationRespose authResponse = _jwtService.createToken(user);
            return Ok(authResponse);
        }
        else
        {
          
            return Unauthorized("Incorrect password");
        }
    }

}

