namespace DesiMealsAbroad.Infra;
using BCrypt.Net;
public class PasswordHasher
{

    private static string commonSalt = BCrypt.GenerateSalt(12);
   
    // Generate a new random salt for a user.
   
    // Hash a plaintext password using the user's salt.
    public static string HashPassword(string password)
    {
        return BCrypt.HashPassword(password, commonSalt);
    }

    // Verify a plaintext password against the hashed password and salt.
    public static bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Verify(password, hashedPassword);
    }
}


