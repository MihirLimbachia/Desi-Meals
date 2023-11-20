using DesiMealsAbroad.DTO;
using DesiMealsAbroad.Infra;
using DesiMealsAbroad.Repositories;
using DesiMealsAbroad.ServiceContracts;
using Microsoft.AspNetCore.Mvc;
using DesiMealsAbroad.Models ;

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
            _ordersRepository.AddSubscriptionPaymentSessionInformation(postCheckoutSessionDTO.subscription.Id, sessionId);
            return Ok(new { sessionId });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("cancel_subscription")]
    public IActionResult CancelSubsription([FromBody] CancelSubscriptionDTO cancelSubscriptionDTO)
    {
        try
        {
            ApplicationUser user = _userRepository.GetUserByEmail(cancelSubscriptionDTO.Email);
            string customerId = user.StripeCustomerId;
            bool cancelled = _stripePaymentService.CancelSubsciption(customerId, cancelSubscriptionDTO.subscription);
            _ordersRepository.inactivateSubscription(cancelSubscriptionDTO.Email, cancelSubscriptionDTO.subscription.SubscriptionId);
            return Ok(cancelled);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("reactivate_subscription")]
    public IActionResult ReactivateSubscription([FromBody] CancelSubscriptionDTO cancelSubscriptionDTO)
    {
        try
        {
            ApplicationUser user = _userRepository.GetUserByEmail(cancelSubscriptionDTO.Email);
            string customerId = user.StripeCustomerId;

            var subcription = new DesiMealsAbroad.Models.Subscription {
                Id = new Guid(cancelSubscriptionDTO.subscription.SubscriptionId),
                Price = cancelSubscriptionDTO.subscription.Price,
                Name= cancelSubscriptionDTO.subscription.Name,
                StripeProductId = cancelSubscriptionDTO.subscription.StripeProductId,
                SubscriptionType = cancelSubscriptionDTO.subscription.SubscriptionType 
            };
           var sessionId = _stripePaymentService.CreateSubscriptionCheckoutSession(customerId, cancelSubscriptionDTO.Email, subcription);
            _ordersRepository.AddSubscriptionPaymentSessionInformation(subcription.Id, sessionId);
            return Ok(new { sessionId });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }


}
