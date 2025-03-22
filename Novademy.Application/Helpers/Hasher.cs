using System.Security.Cryptography;
using System.Text;

namespace Novademy.Application.Helpers;

public static class Hasher
{
    public static string HashPassword(string password)
    {
        var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}