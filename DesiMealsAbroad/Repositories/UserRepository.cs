using Npgsql;
using System;
using System.Data;
using System.Linq;
using DesiMealsAbroad.DTO;
using DesiMealsAbroad.Infra;
public class UserRepository
{
    private readonly PostgresQueryRunner _queryRunner;

    public UserRepository(PostgresQueryRunner queryRunner)
    {
        _queryRunner = queryRunner;
    }

    public void AddUser(RegisterUserDTO user)
    {
        // Define your SQL INSERT statement
        string sql = "INSERT INTO Users (Name, Email, PhoneNumber, Address, Password) VALUES (@Name, @Email, @PhoneNumber, @Address, @Password)";
        string hashedPassword = PasswordHasher.HashPassword(user.Password);
        var parameters = new NpgsqlParameter[]
        {
                new NpgsqlParameter("@Name", user.Name),
                new NpgsqlParameter("@Email", user.Email),
                new NpgsqlParameter("@PhoneNumber", user.Phone),
                new NpgsqlParameter("@Address", user.Address),
                 new NpgsqlParameter("@Password", hashedPassword),

        };
        _queryRunner.ExecuteNonQuery(sql, parameters);
    }

    public void UpdateUser(ApplicationUser user)
    {
        string sql = "UPDATE Users SET Name = @Name, Email = @Email, PhoneNumber = @PhoneNumber, Address = @Address WHERE UserId = @UserId";

        var parameters = new NpgsqlParameter[]
        {
                new NpgsqlParameter("@Name", user.Name),
                new NpgsqlParameter("@Email", user.Email),
                new NpgsqlParameter("@PhoneNumber", user.PhoneNumber),
                new NpgsqlParameter("@Address", user.Address)
        };
        _queryRunner.ExecuteNonQuery(sql, parameters);
    }

    public void DeleteUser(int userId)
    {
        string sql = "DELETE FROM Users WHERE UserId = @UserId";
        var parameters = new NpgsqlParameter[] {
            new NpgsqlParameter("@UserId", userId)
        };
        _queryRunner.ExecuteNonQuery(sql, parameters);   
    }

    public ApplicationUser? GetUserByEmail(string email)
    {
        string sql = "SELECT * FROM Users WHERE email = @Email";
 
        var parameters = new NpgsqlParameter[] {
            new NpgsqlParameter("@Email", email)
        };
        DataTable dataTable = _queryRunner.ExecuteQuery(sql, parameters);
        if (dataTable.Rows.Count > 0)
        {
            var row = dataTable.Rows[0];
            return new ApplicationUser
            {
                Id = (Guid)row["UserId"],
                Name = row["Name"].ToString(),
                Password = row["Password"].ToString(),
                Email = row["Email"].ToString(),
                PhoneNumber = row["PhoneNumber"].ToString(),
                Address = row["Address"].ToString(),
            };
        }
        else
        {
            return null;
        }
    }
}

