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
}

