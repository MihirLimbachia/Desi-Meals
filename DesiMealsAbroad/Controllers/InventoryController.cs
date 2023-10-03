using Microsoft.AspNetCore.Mvc;
using DesiMealsAbroad.Infra;
using DesiMealsAbroad.Models;
using DesiMealsAbroad.DTO;
using System.Data;
using Microsoft.AspNetCore.Authorization;
using Npgsql;


// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860


[Route("api/inventory")]
[Authorize]
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

            // Process the result, which is a DataTable containing the retrieved data
            foreach (DataRow row in result.Rows)
            {
                // Access data in the row as needed
                var dish = new Dish
                {
                    ID = Convert.ToInt32(row["dishid"]),
                    Name = row["name"].ToString(),
                    Description = row["description"].ToString(),
                    Price = Convert.ToDecimal(row["price"]) 
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

            // Construct an SQL query to insert the new dish into the database
            string sqlQuery = "INSERT INTO public.dishes (name, description, price) VALUES (@Name, @Description, @Price)";

            // Create an array of NpgsqlParameters to pass values to the SQL query
            NpgsqlParameter[] parameters = new NpgsqlParameter[]
            {
            new NpgsqlParameter("@Name", dish.Name),
            new NpgsqlParameter("@Description", dish.Description),
            new NpgsqlParameter("@Price", dish.Price)
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