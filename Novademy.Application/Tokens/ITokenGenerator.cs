using Novademy.Application.Models;

namespace Novademy.Application.Tokens;

public interface ITokenGenerator
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
}