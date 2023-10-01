using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DesiMealsAbroad.Postgres;
using DesiMealsAbroad.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Data;

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
}


