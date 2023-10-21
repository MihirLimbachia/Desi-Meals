using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Core;
using DesiMealsAbroad.DTO;
using DesiMealsAbroad.ServiceContracts;
using DesiMealsAbroad.Repositories;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DesiMealsAbroad.Controllers;


[ApiController]
[Route("api/orders")]
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

   

    [HttpPost("createOrder")]
    public IActionResult CreateOrder([FromBody] PostOrdersDTO postOrdersDTO)
    {
        // You can use the Stripe API to retrieve payment details based on session_id

        Guid orderId = Guid.NewGuid();
        List<CartItemDTO> cartItems = _ordersRepository.GetPaymentSessionOrderItems(postOrdersDTO.sessionId);
        if (cartItems != null) {

            _ordersRepository.createOrder(postOrdersDTO.Email, orderId, cartItems);
            _ordersRepository.populateOrderItems(orderId, cartItems);

        } 
        // Return a response to the frontend
        return Ok(new { orderId });
    }
}

