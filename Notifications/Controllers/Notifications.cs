using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notifications.DTO;
using Newtonsoft.Json;
namespace Notifications.Controllers;


[ApiController]
[AllowAnonymous]
[Route("/api/notifications")]
public class Notifications : Controller
{
    private readonly RabbitMQService _rabbitMQService;

    public Notifications(RabbitMQService rabbitMQService)
    {
        _rabbitMQService = rabbitMQService ?? throw new ArgumentNullException(nameof(rabbitMQService));
    }

    [HttpPost("/order-status")]
    public IActionResult sendOrderStatusEmail([FromBody] sendEmailDTO dto)
    {
        try
        {
            string jsonDto = JsonConvert.SerializeObject(dto);
            _rabbitMQService.PublishMessage(jsonDto);
            return Ok("Message published successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error publishing message: {ex.Message}");
        }
    }
}

