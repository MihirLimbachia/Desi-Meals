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
    private readonly OrdersRepository _ordersRepository;


    public PaymentController(ILogger<UserController> logger, IJWTService jwtService, OrdersRepository ordersRepository)
    {
        _logger = logger;
        _jwtService = jwtService;
        _ordersRepository = ordersRepository;
        _stripePaymentService = new StripePaymentService();
    }

    [HttpPost]
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
}
