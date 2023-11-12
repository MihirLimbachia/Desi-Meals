using DesiMealsAbroad.DTO;
using DesiMealsAbroad.Infra;
using DesiMealsAbroad.Repositories;
using DesiMealsAbroad.ServiceContracts;
using Microsoft.AspNetCore.Mvc;

namespace DesiMealsAbroad.Controllers;

[Route("api/payment")]
[ApiController]
public class PaymentController : ControllerBase
{
    private StripePaymentService _stripePaymentService;
    private readonly ILogger<UserController> _logger;
    private readonly IJWTService _jwtService;
    private readonly UserRepository _userRepository ;
    private readonly OrdersRepository _ordersRepository;


    public PaymentController(ILogger<UserController> logger, IJWTService jwtService, OrdersRepository ordersRepository, UserRepository userRepository)
    {
        _logger = logger;
        _jwtService = jwtService;
        _ordersRepository = ordersRepository;
        _stripePaymentService = new StripePaymentService();
        _userRepository = userRepository;
    }

    [HttpPost("orders")]
    public IActionResult CreateCheckoutSession([FromBody] PostPaymentsDTO postPaymentsDTO)
    {
        List<CartItemDTO> items = postPaymentsDTO.CartItems;
        try
        {
            var sessionId = _stripePaymentService.CreateCheckoutSession(items);
            _ordersRepository.AddPaymentSessionOrderItems(postPaymentsDTO, sessionId);

            return Ok(new { sessionId });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("subscriptions")]
    public IActionResult CreateSubscriptionCheckoutSession([FromBody] PostCheckoutSessionDTO postCheckoutSessionDTO)
    {
        try
        {
            var user = _userRepository.GetUserByEmail(postCheckoutSessionDTO.Email);
            string customerId = user.StripeCustomerId;
            var sessionId = _stripePaymentService.CreateSubscriptionCheckoutSession(customerId, postCheckoutSessionDTO.Email, postCheckoutSessionDTO.subscription);
            return Ok(new { sessionId });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("subscriptions")]
    public IActionResult getSubscriptionByEmail([FromQuery] string email)
    {
        try
        {
            var user = _userRepository.GetUserByEmail(email);
            string customerId = user.StripeCustomerId;
            var subscriptions = _stripePaymentService.GetSubscriptionsByUser(customerId);
            return Ok(subscriptions);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
