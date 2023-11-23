namespace Notifications.Infra;
using Notifications.DTO;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public class EmailWorker : BackgroundService
{
    private const string ExchangeName = "notifications";
    private const string RoutingKey = "email_notifications";
    private const string QueueName = "email_notifications";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var connection = factory.CreateConnection();
                var channel = connection.CreateModel();
               
                channel.ExchangeDeclare(ExchangeName, ExchangeType.Topic, durable: true);
                channel.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
                channel.QueueBind(QueueName, ExchangeName, RoutingKey);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        var emailData = JsonConvert.DeserializeObject<sendEmailDTO>(message);
                        Console.WriteLine($"Consumed message: {emailData.email} {emailData.subject} {emailData.message}");
                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing message: {ex.Message}");
                        Console.WriteLine($"Raw message content: {Encoding.UTF8.GetString(ea.Body.ToArray())}");
                        channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                    }
                };

                channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);
                
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in the background service: {ex.Message}");
            }
        }
    }
}

