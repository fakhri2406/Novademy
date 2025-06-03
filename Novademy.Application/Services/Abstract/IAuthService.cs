using System;
using System.Threading.Tasks;
using Novademy.Contracts.Requests.Auth;
using Novademy.Contracts.Responses.Auth;
using Novademy.Application.Models;

namespace Novademy.Application.Services.Abstract;

public interface IAuthService
{
    Task<Guid> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task VerifyEmailAsync(VerifyEmailRequest request);
    Task<AuthResponse> RefreshAsync(RefreshTokenRequest request);
    Task LogoutAsync(Guid userId);
    Task<User> GetUserByIdAsync(Guid userId);
} 