namespace DesiMealsAbroad.Infra;
using DesiMealsAbroad.Models;
using DesiMealsAbroad.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

public class OrderGenerationService : BackgroundService
 {
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _runInterval = TimeSpan.FromMinutes(1);
        private readonly ILogger<OrderGenerationService> _logger;

        public OrderGenerationService(IServiceProvider serviceProvider, ILogger<OrderGenerationService> logger)
         {
                _serviceProvider = serviceProvider;
                _logger = logger;
         }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
        _logger.LogInformation("OrderGenerationService is running.");

         while (!stoppingToken.IsCancellationRequested)
            {
             try
             {
                var now = DateTime.UtcNow;
                var nextRunTime = now + _runInterval;

                var delayTime = nextRunTime - now;
                await Task.Delay(delayTime, stoppingToken);

                _logger.LogInformation("OrderGenerationService is running."+ now);
                using (var scope = _serviceProvider.CreateScope())
                {

                    var ordersRepository = scope.ServiceProvider.GetRequiredService<OrdersRepository>();
                    List<UserSubscription> subscriptions = ordersRepository.getDueSubscriptions();
                    
                    foreach (var subscription in subscriptions)
                    {
                        _logger.LogInformation("OrderGenerationService is running." + subscription.SubscriptionId);
                        Subscription? subDetails = ordersRepository.GetSubscriptionDetails(subscription.SubscriptionId);
                        if (subDetails is null) return;
                        var subscriptionOrder = new SubscriptionOrder
                        {
                            UserSubscriptionId = subscription.UserSubscriptionId,
                            SubscriptionName = subDetails.Name,
                            CreateDate = DateTime.UtcNow,
                            SubscriptionDescription = subDetails.Description,
                            NoOfMeals = subDetails.NoOfMeals,
                            ImgUrl = subDetails.ImgUrl,
                            Price = subDetails.Price,
                            SubscriptionType = subDetails.SubscriptionType
                        };
                        _logger.LogInformation("OrderGenerationService is running." + subDetails.Name);
                        Guid orderId = ordersRepository.createSubscriptionOrder(subscriptionOrder);
                        _logger.LogInformation("Created order"+ orderId);

                    }
                }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.StackTrace);
                }
            }
        }
 }