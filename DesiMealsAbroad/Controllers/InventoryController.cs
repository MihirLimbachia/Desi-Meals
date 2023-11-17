using Microsoft.AspNetCore.Mvc;
using DesiMealsAbroad.Infra;
using DesiMealsAbroad.Models;
using DesiMealsAbroad.DTO;
using System.Data;
using Microsoft.AspNetCore.Authorization;
using Npgsql;


// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860


[Route("api/inventory")]
[ApiController]
public class InventoryController : ControllerBase
{
    private readonly PostgresQueryRunner _postgresQueryRunner; // Inject your DbContext

    public InventoryController(PostgresQueryRunner postgresQueryRunner)
    {
        _postgresQueryRunner = postgresQueryRunner;
    }


    // GET api/inventory/dishes
    [HttpGet("dishes")] // Define a route for the GET endpoint to retrieve dishes
    public IActionResult GetDishes()
    {
        try
        {
         
            string sqlQuery = "SELECT * FROM public.dishes";
            List<Dish> dishes = new List<Dish>();
            var result = _postgresQueryRunner.ExecuteQuery(sqlQuery);

            foreach (DataRow row in result.Rows)
            {
               
                List<string> ingredients = row["ingredients"].ToString().Split(",", StringSplitOptions.RemoveEmptyEntries)
                                       .Select(s => s.Trim()) 
                                       .ToList();
                var dish = new Dish
                {
                    ID = Convert.ToInt32(row["dishid"]),
                    Name = row["name"].ToString(),
                    Description = row["description"].ToString(),
                    ImgUrl = row["imgUrl"].ToString(),
                    Price = Convert.ToDecimal(row["price"]) ,
                    Calories = Convert.ToDecimal(row["calories"]),
                    Ingredients = ingredients,
                    Origin = row["origin"].ToString(),
                    SpiceLevel = row["spice_level"].ToString()
                     
                };
                dishes.Add(dish);
            }
            
            // Return the list of dishes as a JSON response
            return Ok(dishes);
        }
        catch (Exception ex)
        { 
            return StatusCode(500, $"Internal server error while getting dishes: {ex.Message}");
        }
    }


    // GET api/inventory/subscriptions
    [HttpGet("subscriptions")]
    public IActionResult GetSubscriptions()
    {
        try
        {
            string sqlQuery = "SELECT * FROM public.subscription";
            List<Subscription> subscriptions = new List<Subscription>();
            var result = _postgresQueryRunner.ExecuteQuery(sqlQuery);

            foreach (DataRow row in result.Rows)
            {
                var subscription = new Subscription
                {
                    Id = Guid.Parse(row["id"].ToString()),
                    Name = row["name"].ToString(),
                    Description = row["description"].ToString(),
                    NoOfMeals = row["no_of_meals"] != DBNull.Value ? Convert.ToInt32(row["no_of_meals"]) : (int?)null,
                    ImgUrl = row["imgurl"].ToString(),
                    Price = row["price"] != DBNull.Value ? Convert.ToDecimal(row["price"]) : (decimal?)null,
                    Duration = row["duration"].ToString(),
                    SubscriptionType = row["subscription_type"].ToString(),
                    Contents = row["contents"].ToString(),
                    StripeProductId = row["stripe_product_id"].ToString()
                };

                subscriptions.Add(subscription);
            }

            // Return the list of subscriptions as a JSON response
            return Ok(subscriptions);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error while getting subscriptions: {ex.Message}");
        }
    }

    [HttpPost("dishes")] // Define a route for the POST endpoint to create a new dish
    public IActionResult CreateDish([FromBody] AddDishDTO dish)
    {
        try
        {
            // Validate the incoming dish object (you can add more validation as needed)
            if (dish == null)
            {
                return BadRequest("Invalid dish data");
            }
            string ingredientsString = string.Join(",", dish.Ingredients);
            // Construct an SQL query to insert the new dish into the database
            string sqlQuery = "INSERT INTO public.dishes (name, description, price, imgUrl, calories, ingredients) VALUES (@Name, @Description, @Price,  @ImgUrl, @Calories, " +
                "@Ingredients)";

            // Create an array of NpgsqlParameters to pass values to the SQL query
            NpgsqlParameter[] parameters = new NpgsqlParameter[]
            {
            new NpgsqlParameter("@Name", dish.Name),
            new NpgsqlParameter("@Description", dish.Description),
            new NpgsqlParameter("@Price", dish.Price),
            new NpgsqlParameter("@ImgUrl", dish.ImgUrl),
            new NpgsqlParameter("@Calories", dish.Calories),
            new NpgsqlParameter("@Ingredients", ingredientsString)

            };

            // Execute the SQL query to insert the new dish
            _postgresQueryRunner.ExecuteNonQuery(sqlQuery, parameters);

            // Optionally, you can return the newly created dish or a success message
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error while creating a dish: {ex.Message}");
        }
    }

}