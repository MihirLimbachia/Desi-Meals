namespace DesiMealsAbroad.Infra;
using Stripe;
using Stripe.Checkout;
using DesiMealsAbroad.DTO;
using System.Collections.Generic;

public class StripePaymentService
{

    public StripePaymentService() {
        StripeConfiguration.ApiKey = "sk_test_51O2hmhHDd6RSPek2pUhp5hjiwWsLVAjZ65ZX1r9xaRdz3Js6OpAzyg1epUgftZGE3EdWGVoCEz6lLWpg6gwGX3U900BTPbRYhy";
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
            SuccessUrl = "http://localhost:4200/payment-success?session_id={CHECKOUT_SESSION_ID}", // Replace with your success URL
            CancelUrl = "https://localhost:4200/payment-failure", // Replace with your cancel URL
        };

        var service = new SessionService();
        var session = service.Create(options);

        return session.Id;
    }

}
