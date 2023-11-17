namespace DesiMealsAbroad.Infra;
using Stripe;
using Stripe.Checkout;
using DesiMealsAbroad.DTO;
using System.Collections.Generic;
using DesiMealsAbroad.Models;

public class StripePaymentService
{

    public StripePaymentService() {
        StripeConfiguration.ApiKey = "sk_test_51O2hmhHDd6RSPek2pUhp5hjiwWsLVAjZ65ZX1r9xaRdz3Js6OpAzyg1epUgftZGE3EdWGVoCEz6lLWpg6gwGX3U900BTPbRYhy";
    }


    public string CreateCustomer(string email) {

        var customerOptions = new CustomerCreateOptions
        {
            Email = email,
            Description = $"Customer for {email}",
        };

        var customerService = new CustomerService();
        var customer =  customerService.Create(customerOptions);
        Console.WriteLine(customer);
        return customer.Id;
    }

    public string CreateCheckoutSession(List<CartItemDTO> items)
    {
      
       var lineItems = new List<SessionLineItemOptions>();

        foreach (var cartItem in items)
        {
            var dish = cartItem.Item;
            var lineItem = new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = "usd", 
                    UnitAmount = (long)(dish.Price * 100), 
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = dish.Name,
                        Description = dish.Description,
                        Images = new List<string> { dish.ImgUrl },
                    },
                },
                Quantity = cartItem.Quantity,
            };
            lineItems.Add(lineItem);
        }
        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = lineItems,
            Mode = "payment",
            SuccessUrl = "http://localhost:4200/payment-success?session_id={CHECKOUT_SESSION_ID}", 
            CancelUrl = "https://localhost:4200/payment-failure",
        };

        var service = new SessionService();
        var session = service.Create(options);

        return session.Id;
    }

    public bool CancelSubsciption(string customerId, DesiMealsAbroad.Models.UserSubscriptionDetails subscription)
    {
        try
        {
          
            var service = new SubscriptionService();
            var subscriptions = service.List(new SubscriptionListOptions
            {
                Customer = customerId,
                Status = "active",
            });
        
            foreach (var sub in subscriptions)
            {
               
                if (sub.Items?.Data.Any(item => item.Price.ProductId == subscription.StripeProductId) == true)
                {
                  
                    var cancelOptions = new SubscriptionCancelOptions { };
                    service.Cancel(sub.Id, cancelOptions);
                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return false;
        }
    }

    public string CreateSubscriptionCheckoutSession(string customerId, string email, DesiMealsAbroad.Models.Subscription subscription)
    {

        var productId = subscription.StripeProductId;
        var priceOptions = new PriceCreateOptions
        {
            Product = productId,
            UnitAmountDecimal = subscription.Price * 100,
            Currency = "usd",
            Recurring = new PriceRecurringOptions
            {
                Interval = "month",
            },
        };
        var priceService = new PriceService();
        var price = priceService.Create(priceOptions);
        var options = new SessionCreateOptions
        {
            Customer = customerId,
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    Price = price.Id,
                    Quantity = 1,
                },
            },
            Mode = "subscription",
            SuccessUrl = "http://localhost:4200/subscription-success?session_id={CHECKOUT_SESSION_ID}",
            CancelUrl = "https://localhost:4200/subscription-failure", 
        };

        var sessionService = new SessionService();
        var session = sessionService.Create(options);
        return session.Id;

    }

    public StripeList<Stripe.Subscription> GetSubscriptionsByUser(string customerId)
    {

        var subscriptionOptions = new SubscriptionListOptions
        {
            Customer = customerId,
        };

        var subscriptionService = new SubscriptionService();
        StripeList<Stripe.Subscription> subscriptions = subscriptionService.List(subscriptionOptions);

        return subscriptions;

    }

}
