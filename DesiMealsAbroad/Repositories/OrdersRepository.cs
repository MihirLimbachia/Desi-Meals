using System;
using DesiMealsAbroad.DTO;
using DesiMealsAbroad.Infra;
using Npgsql;
using System.Data;
using Newtonsoft.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

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


    public void createOrder(string email, Guid orderId, List<CartItemDTO> cartItems)
    {
       
        decimal orderTotal = CalculateOrderTotal(cartItems);
        string sql = "INSERT INTO orders (order_id, email, order_date, total_amount, status) VALUES (@OrderId, @Email, @OrderDate,@Amount,@Status)";
        var parameters = new NpgsqlParameter[]
        {
            new NpgsqlParameter("@OrderId", orderId),
            new NpgsqlParameter("@Email", email),
            new NpgsqlParameter("@Amount", orderTotal),
            new NpgsqlParameter("@status", "CREATED"),
            new NpgsqlParameter("@OrderDate", DateTime.UtcNow),

        };
        _queryRunner.ExecuteNonQuery(sql, parameters);
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
}

