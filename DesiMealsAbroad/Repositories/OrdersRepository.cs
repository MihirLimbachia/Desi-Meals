using System;
using DesiMealsAbroad.DTO;
using DesiMealsAbroad.Models;
using DesiMealsAbroad.Infra;
using Npgsql;
using System.Data;
using Newtonsoft.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using NpgsqlTypes;
using Microsoft.AspNetCore.Mvc;

namespace DesiMealsAbroad.Repositories;

public class OrdersRepository
{

    private readonly PostgresQueryRunner _queryRunner;

    public OrdersRepository(PostgresQueryRunner queryRunner)
    {
        _queryRunner = queryRunner;
    }

    public void AddPaymentSessionOrderItems(PostPaymentsDTO postPaymentsDTO, string sessionId)
    {
        List<CartItemDTO> items = postPaymentsDTO.CartItems;
        string jsonLineItems = JsonConvert.SerializeObject(items);

        string sql = "INSERT INTO paymentSessionDetails (sessionId, email,lineitems) VALUES (@SessionId, @Email, @LineItems::jsonb)";
        var parameters = new NpgsqlParameter[]
        {
            new NpgsqlParameter("@sessionId", sessionId),
            new NpgsqlParameter("@Email", postPaymentsDTO.Email),
            new NpgsqlParameter("@LineItems", jsonLineItems)

        };
        _queryRunner.ExecuteNonQuery(sql, parameters);
    }


    public void AddSubscriptionPaymentSessionInformation(Guid subscriptionId, string sessionId)
    {
     
        string sql = "INSERT INTO subscription_payment_session (session_id, subscription_id) VALUES (@SessionId, @SubscriptionId)";
        var parameters = new NpgsqlParameter[]
        {
            new NpgsqlParameter("@SessionId", sessionId),
            new NpgsqlParameter("@SubscriptionId",subscriptionId)

        };
        _queryRunner.ExecuteNonQuery(sql, parameters);
    }

    public decimal CalculateOrderTotal(List<CartItemDTO> items)
    {
        decimal total = 0;

        foreach (var cartItem in items)
        {
            if (cartItem.Item != null)
            {
                decimal subtotal = cartItem.Quantity * cartItem.Item.Price;
                total += subtotal;
            }
        }

        return total;
    }


    public bool inactivateSubscription(string email, string subscriptionId)
    {

        string sql = "update user_subscription set status='inactive', cancelled_at =@CurrentDate where user_email=@Email and subscription_id = @SubscriptionId";
        var parameters = new NpgsqlParameter[]
       {
            new NpgsqlParameter("@Email", email),
            new NpgsqlParameter("@SubscriptionId", subscriptionId),
            new NpgsqlParameter("@CurrentDate", NpgsqlDbType.Date) { Value = DateTime.UtcNow }
       };
       
        _queryRunner.ExecuteNonQuery(sql, parameters);
        return true;
    }

    public Guid createOrder(string email, Guid orderId, List<CartItemDTO> cartItems, string sessionId)
    {
       
        decimal orderTotal = CalculateOrderTotal(cartItems);
        List<OrderItem> orderItems = convertCartItemsToOrderItems(orderId, cartItems);

        string sql1 = "select order_id from orders where session_id=@SessionId";
        var parameters1 = new NpgsqlParameter[]
       {
            new NpgsqlParameter("@SessionId", sessionId),
       };
        DataTable dataTable = _queryRunner.ExecuteQuery(sql1, parameters1);
        if (dataTable.Rows.Count > 0)
        {

            var row = dataTable.Rows[0];
            Guid existingOrderId = new Guid(row["order_id"].ToString());
            return existingOrderId;
        }
        string orderItemsJson = JsonConvert.SerializeObject(orderItems);
        string sql = "INSERT INTO orders (order_id, email, order_date, total_amount, status, order_items, session_id) VALUES (@OrderId, @Email, @OrderDate,@Amount,@Status, @OrderItems::jsonb, @SessionId)";
        var parameters = new NpgsqlParameter[]
        {
            new NpgsqlParameter("@OrderId", orderId),
            new NpgsqlParameter("@Email", email),
            new NpgsqlParameter("@Amount", orderTotal),
            new NpgsqlParameter("@status", "CREATED"),
            new NpgsqlParameter("@OrderDate", DateTime.UtcNow),
            new NpgsqlParameter("@OrderItems", orderItemsJson),
            new NpgsqlParameter("@SessionId", sessionId),
        };
        _queryRunner.ExecuteNonQuery(sql, parameters);
        return orderId;
    }

    public Subscription? GetSubscriptionDetails(string subscription_id)
    {
        try
        {
            string sqlQuery = "SELECT * FROM public.subscription where id = @SubscriptionId::uuid";
              var parameters1 = new NpgsqlParameter[]
              {
                    new NpgsqlParameter("@SubscriptionId", subscription_id),
              };
            DataTable dataTable = _queryRunner.ExecuteQuery(sqlQuery, parameters1);

            if (dataTable.Rows.Count > 0)
            {

                var row = dataTable.Rows[0];
                var subscription = new Subscription
                {
                    Id = Guid.Parse(row["id"].ToString()),
                    Name = row["name"].ToString(),
                    Description = row["description"].ToString(),
                    NoOfMeals = Convert.ToInt32(row["no_of_meals"]),
                    ImgUrl = row["imgurl"].ToString(),
                    Price = Convert.ToDecimal(row["price"]),
                    Duration = row["duration"].ToString(),
                    SubscriptionType = row["subscription_type"].ToString(),
                    Contents = row["contents"].ToString(),
                    StripeProductId = row["stripe_product_id"].ToString()
                };
                return subscription;

            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.StackTrace);
            return null;
        }
    }


    public Guid createSubscriptionOrder(SubscriptionOrder subscriptionOrder)
    {
        Guid orderId = Guid.NewGuid();
        string sql = @"INSERT INTO subscription_orders 
                    (id, user_subscription_id, subscription_name, subscription_description, 
                     no_of_meals, imgurl, price, subscription_type, create_date) 
                  VALUES 
                    (@Id, @UserSubscriptionId, @SubscriptionName, @SubscriptionDescription, 
                     @NoOfMeals, @ImgUrl, @Price, @SubscriptionType, @CreateDate)";

        var parameters = new NpgsqlParameter[]
        {
        new NpgsqlParameter("@Id", orderId),
        new NpgsqlParameter("@UserSubscriptionId", subscriptionOrder.UserSubscriptionId),
        new NpgsqlParameter("@SubscriptionName", subscriptionOrder.SubscriptionName),
        new NpgsqlParameter("@SubscriptionDescription", subscriptionOrder.SubscriptionDescription),
        new NpgsqlParameter("@NoOfMeals", subscriptionOrder.NoOfMeals),
        new NpgsqlParameter("@ImgUrl", subscriptionOrder.ImgUrl),
        new NpgsqlParameter("@Price", subscriptionOrder.Price),
        new NpgsqlParameter("@SubscriptionType", subscriptionOrder.SubscriptionType),
        new NpgsqlParameter("@CreateDate", subscriptionOrder.CreateDate),
        };
        _queryRunner.ExecuteNonQuery(sql, parameters);
        return orderId;
    }

    public Guid createSubscription(string email, string subscriptionId,  string sessionId)
    {

        string sql1 = "select user_subscription_id from user_subscription where session_id=@SessionId";
        var parameters1 = new NpgsqlParameter[]
       {
            new NpgsqlParameter("@SessionId", sessionId),
       };
        DataTable dataTable = _queryRunner.ExecuteQuery(sql1, parameters1);
        if (dataTable.Rows.Count > 0)
        {

            var row = dataTable.Rows[0];
            Guid existingSubscriptionId = new Guid(row["user_subscription_id"].ToString());
            return existingSubscriptionId;
        }
        Guid newSubscriptionId = Guid.NewGuid();
        string sql = "INSERT INTO user_subscription (user_subscription_id, user_email, subscription_id, session_id, status) VALUES (@User_subscription_id,@User_email, @Subscription_id, @SessionId, @Status)";
        var parameters = new NpgsqlParameter[]
        {
            new NpgsqlParameter("@User_subscription_id", newSubscriptionId),
            new NpgsqlParameter("@User_email", email),
            new NpgsqlParameter("@Subscription_id", subscriptionId),
            new NpgsqlParameter("@Status", "Active"),
            new NpgsqlParameter("@SessionId", sessionId),
        };
        _queryRunner.ExecuteNonQuery(sql, parameters);
        return newSubscriptionId;
    }

    public List<UserSubscription> getDueSubscriptions()
    {

        string sql1 = "select usb.* from user_subscription usb where status='Active'";
        
        DataTable dataTable = _queryRunner.ExecuteQuery(sql1);
        List<UserSubscription> dueSubscriptions = new List<UserSubscription>();
        if (dataTable.Rows.Count > 0)
        {
            foreach (DataRow row in dataTable.Rows)
            {
                var userSubscription = new UserSubscription
                {
                    UserSubscriptionId = new Guid(row["user_subscription_id"].ToString()),
                    Status = row["status"].ToString(),
                    CreatedAt = DateTime.Parse(row["created_at"].ToString()),

                    SubscriptionId = row["subscription_id"].ToString()
                };

                if (GetOrderCount(userSubscription.UserSubscriptionId, userSubscription.CreatedAt) < CalculateRequiredOrders(userSubscription.CreatedAt, 1))
                {
                    dueSubscriptions.Add(userSubscription);
                }
            }
        }
        return dueSubscriptions;
    }

    private int CalculateRequiredOrders(DateTime createdDate, int requiredOrdersPerMonth)
    {
        int daysPassed = (int)(DateTime.UtcNow - createdDate).TotalDays;
        int requiredOrders = (int)Math.Ceiling((double)daysPassed / 30) * requiredOrdersPerMonth;
        return requiredOrders;
    }


    private int GetOrderCount(Guid userSubscriptionId, DateTime createdDate)
    { 
        string orderCountQuery = "SELECT COUNT(*) as order_count FROM subscription_orders WHERE user_subscription_id = @UserSubscriptionId AND create_date >= @CreatedDate";
        var parameters = new NpgsqlParameter[]
        {
        new NpgsqlParameter("@UserSubscriptionId", userSubscriptionId),
        new NpgsqlParameter("@CreatedDate", createdDate),
        };

        DataTable dataTable = _queryRunner.ExecuteQuery(orderCountQuery, parameters);
        if (dataTable.Rows.Count > 0)
        {
            var row = dataTable.Rows[0];
            int order_count = Convert.ToInt32(row["order_count"]);
            return order_count;
        }
        else
        {
            return 0;
        }
    }

    public List<OrderItem> convertCartItemsToOrderItems (Guid orderId, List<CartItemDTO> cartItems)
    {
        List<OrderItem> orderItems= new List<OrderItem>();

        foreach (CartItemDTO item in cartItems)
        {
            var orderItem =  new OrderItem {
            OrderId = orderId,
            ItemId = item.Item.ID,
            Quantity = item.Quantity,
            Name = item.Item.Name,
            Price  = item.Item.Price,
            ImgUrl = item.Item.ImgUrl
            };
            orderItems.Add(orderItem);
        }
        return orderItems;
    }

    public void populateOrderItems(Guid orderId, List<CartItemDTO> cartItems)
    {
    
        foreach (CartItemDTO cartItem in cartItems)
        {
           string sql = "INSERT INTO public.orderItems (order_id, item_id, quantity, price) VALUES (@order_id, @item_id, @quantity, @price)";
           var parameters = new NpgsqlParameter[]
           {
                new NpgsqlParameter("@order_id", orderId),
                new NpgsqlParameter("@item_id", cartItem.Item.ID),
                new NpgsqlParameter("@quantity", cartItem.Quantity),
                new NpgsqlParameter("@price", (double)cartItem.Item.Price)
           };
            _queryRunner.ExecuteNonQuery(sql, parameters);
        }
        
    }

    public List<CartItemDTO>? GetPaymentSessionOrderItems(string sessionId)
    {
        string sql = "SELECT * FROM paymentSessionDetails WHERE sessionId = @SessionId";

        var parameters = new NpgsqlParameter[] {
            new NpgsqlParameter("@SessionId", sessionId)
        };
       
        DataTable dataTable = _queryRunner.ExecuteQuery(sql, parameters);
        if (dataTable.Rows.Count > 0)
        {
            
            var row = dataTable.Rows[0];
            List<CartItemDTO> items = JsonConvert.DeserializeObject<List<CartItemDTO>>(row["lineitems"].ToString());
            return items;
        }
        else
        {
            return null;
        }
    }

    public string? GetPaymentSessionSubscriptionId(string sessionId)
    {
        string sql = "SELECT subscription_id FROM subscription_payment_session WHERE session_id = @SessionId";

        var parameters = new NpgsqlParameter[] {
            new NpgsqlParameter("@SessionId", sessionId)
        };

        DataTable dataTable = _queryRunner.ExecuteQuery(sql, parameters);
        if (dataTable.Rows.Count > 0)
        {
            var row = dataTable.Rows[0];
            return row["subscription_id"].ToString();
        }
        else
        {
            return null;
        }
    }

    public void UpdateCartItems(string email, List<CartItemDTO> cartItems)
    {
        string sql = "UPDATE users set cart_items = @cartItemsJSON::jsonb WHERE email = @Email";
        string cartItemsJson = JsonConvert.SerializeObject(cartItems);
        var parameters = new NpgsqlParameter[] {
            new NpgsqlParameter("@Email", email),
            new NpgsqlParameter("@cartItemsJSON", cartItemsJson)
        };

        _queryRunner.ExecuteNonQuery(sql, parameters);
    }

    public List<CartItemDTO>? GetCartItems(string email)
    {
        string sql = "select cart_items from users WHERE email = @Email";
        var parameters = new NpgsqlParameter[] {
            new NpgsqlParameter("@Email", email),
        };
        DataTable dataTable = _queryRunner.ExecuteQuery(sql, parameters);
        if (dataTable.Rows.Count > 0)
        {

            var row = dataTable.Rows[0];
            List<CartItemDTO> items = JsonConvert.DeserializeObject<List<CartItemDTO>>(row["cart_items"].ToString());
            return items;
        }
        return null;
    }

    public List<Order>? GetOrders(string email, DateTime startDate, DateTime endDate)
    {
        string sql = "SELECT * FROM orders WHERE email = @Email and order_date >= @StartDate and order_date<= @EndDate";
        var parameters = new NpgsqlParameter[] {
            new NpgsqlParameter("@Email", email),
            new NpgsqlParameter("@StartDate", startDate),
            new NpgsqlParameter("@EndDate", endDate)
        };
        DataTable dataTable = _queryRunner.ExecuteQuery(sql, parameters);
        List<Order> orders = new List<Order>();
        if (dataTable.Rows.Count > 0)
        {
            foreach (DataRow row in dataTable.Rows)
            {
                Guid orderId = new Guid(row["order_id"].ToString());
                var order = new Order
                {
                    OrderId = orderId,
                    Email = row["email"].ToString(),
                    Status = row["status"].ToString(),
                    OrderDate = DateTime.Parse(row["order_date"].ToString()),
                    TotalAmount = (decimal)row["total_amount"],
                    OrderItems = JsonConvert.DeserializeObject<List<OrderItem>>(row["order_items"].ToString())
                };
                orders.Add(order);
            }
            return orders;
        }
        else
        {
            return null;
        }
    }

    public List<UserSubscriptionDetails>? GetUserSubcriptions(string email)
    {
        string sql = "SELECT * FROM user_subscription WHERE user_email = @Email";

        var parameters = new NpgsqlParameter[] {
            new NpgsqlParameter("@Email", email)
        };


        DataTable dataTable = _queryRunner.ExecuteQuery(sql, parameters);
        List<UserSubscriptionDetails> subscriptions = new List<UserSubscriptionDetails>();
        if (dataTable.Rows.Count > 0)
        {
            foreach (DataRow row in dataTable.Rows)
            {
                Guid id = new Guid(row["user_subscription_id"].ToString());
                var subscription = new UserSubscription
                {
                    UserSubscriptionId = id,
                    Status = row["status"].ToString(),
                    CreatedAt = DateTime.Parse(row["created_at"].ToString()),
                    SubscriptionId = row["subscription_id"].ToString(),
                    CancelledAt = string.IsNullOrEmpty(row["cancelled_at"].ToString()) ? (DateTime?)null : DateTime.Parse(row["cancelled_at"].ToString())
                 };
                string sqlQuery = "SELECT * FROM public.subscription where id = @SubscriptionId::uuid";
                var parameters2 = new NpgsqlParameter[] {
                    new NpgsqlParameter("@SubscriptionId", subscription.SubscriptionId)
                };

                var result = _queryRunner.ExecuteQuery(sqlQuery, parameters2);

                var subscriptionRow = result.Rows[0];
                
                var subscriptionDetails = new UserSubscriptionDetails
                {
                    SubscriptionId = subscriptionRow["id"].ToString(),
                    UserSubscriptionId = subscription.UserSubscriptionId,
                    Status = subscription.Status,
                    CreatedAt = subscription.CreatedAt,
                    CancelledAt = subscription.CancelledAt,
                    Name = subscriptionRow["name"].ToString(),
                    NoOfMeals = Convert.ToInt32(subscriptionRow["no_of_meals"]),
                    ImgUrl = subscriptionRow["imgurl"].ToString(),
                    Price = Convert.ToDecimal(subscriptionRow["price"]),
                    SubscriptionType = subscriptionRow["subscription_type"].ToString(),
                    StripeProductId = subscriptionRow["stripe_product_id"].ToString()
                };

                subscriptions.Add(subscriptionDetails);
            }
            return subscriptions;

        }
        else
        {
            return null;
        }
    }
}

