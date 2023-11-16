using DesiMealsAbroad.DTO;
using DesiMealsAbroad.Models;
using DesiMealsAbroad.ServiceContracts;
using DesiMealsAbroad.Repositories;
using Microsoft.AspNetCore.Mvc;
namespace DesiMealsAbroad.Controllers;

[ApiController]
[Route("api/")]
public class OrdersController : Controller
{
    private readonly ILogger<UserController> _logger;
    private readonly IJWTService _jwtService;
    private readonly OrdersRepository _ordersRepository;

    public OrdersController(ILogger<UserController> logger, IJWTService jwtService, OrdersRepository ordersRepository)
    {
        _logger = logger;
        _jwtService = jwtService;
        _ordersRepository = ordersRepository;
    }

    [HttpPost("orders/createOrder")]
    public IActionResult CreateOrder([FromBody] PostOrdersDTO postOrdersDTO)
    {
      
        Guid orderId = Guid.NewGuid();
        List<CartItemDTO>? cartItems = _ordersRepository.GetPaymentSessionOrderItems(postOrdersDTO.sessionId);
        if (cartItems != null) {

            Guid createdOrderId = _ordersRepository.createOrder(postOrdersDTO.Email, orderId, cartItems, postOrdersDTO.sessionId);
            if (createdOrderId == orderId) _ordersRepository.populateOrderItems(orderId, cartItems);
            orderId = createdOrderId;
            return Ok(new{ orderId }) ;
        } 
        return Ok(null);
    }


    [HttpPost("orders/createSubscription")]
    public IActionResult CreateSubscription([FromBody] PostSubscriptionDTO postSubscriptionDTO)
    {

        string subscription = _ordersRepository.GetPaymentSessionSubscriptionId(postSubscriptionDTO.sessionId);
        Guid subscriptionId = _ordersRepository.createSubscription(postSubscriptionDTO.Email, subscription, postSubscriptionDTO.sessionId);
        return Ok(new { subscriptionId });

    }

    [HttpPost("cart/updateCartItems")]
    public IActionResult UpdateCartItems([FromBody] UpdateCartItems updateCartItems)
    {

        Guid orderId = Guid.NewGuid();
        try
        {
            _ordersRepository.UpdateCartItems(updateCartItems.Email, updateCartItems.CartItems);
            return Ok(true);
        }
        catch(Exception exception) {
            return StatusCode(500, new { error = "An error occurred while updating cart items "+ exception.Message });
        }   
    }

    [HttpGet("orders/listOrders")]
    public IActionResult getOrder([FromQuery] GetOrdersDTO getOrdersDTO)
    {
       
        List<Order>? cartItems = _ordersRepository.GetOrders(getOrdersDTO.Email, getOrdersDTO.StartDate, getOrdersDTO.EndDate);
     
        return Ok(cartItems);
    }

    [HttpGet("cart/getCartItems")]
    public IActionResult getCartItems([FromQuery] string email )
    {

        List<CartItemDTO>? cartItems = _ordersRepository.GetCartItems(email);

        return Ok(cartItems);
    }

    [HttpGet("orders/subscriptions")]
    public IActionResult getSubscriptionByEmail([FromQuery] string email)
    {
        try
        {
            var subscriptions = _ordersRepository.GetUserSubcriptions(email);
            return Ok(subscriptions);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

