using RabbitMQ.Client;
using System.Text;

public class RabbitMQService
{
    private readonly string rabbitMQConnectionString = "amqp://guest:guest@localhost:5672/"; 
    private readonly string exchangeName = "notifications";
    private readonly string routingKey = "email_notifications";

    public void PublishMessage(string message)
    {
        try
        {
            var factory = new ConnectionFactory
            {
                Uri = new Uri(rabbitMQConnectionString)
            };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchangeName, ExchangeType.Topic, durable: true);
                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(exchange: exchangeName, routingKey: routingKey, basicProperties: null, body: body);
                Console.WriteLine($" [x] Sent '{message}'");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error publishing message: {ex.Message}");    
        }
    }

}
