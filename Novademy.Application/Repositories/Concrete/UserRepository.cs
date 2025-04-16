using Dapper;
using Microsoft.AspNetCore.Http;
using Novademy.Application.Cloudinary;
using Novademy.Application.Data.Dapper;
using Novademy.Application.Helpers;
using Novademy.Application.Models;
using Novademy.Application.Repositories.Abstract;

namespace Novademy.Application.Repositories.Concrete;

public class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IMediaUpload _mediaUpload;
    
    public UserRepository(IDbConnectionFactory connectionFactory, IMediaUpload mediaUpload)
    {
        _connectionFactory = connectionFactory;
        _mediaUpload = mediaUpload;
    }
    
    #region Register
    
    public async Task<User> RegisterUserAsync(User user, IFormFile? profilePicture)
    {
        const string checkUsernameSql = "SELECT COUNT(1) FROM Users WHERE Username = @Username";
        const string checkEmailSql = "SELECT COUNT(1) FROM Users WHERE Email = @Email";
        const string checkPhoneSql = "SELECT COUNT(1) FROM Users WHERE PhoneNumber = @PhoneNumber";
        const string checkAdminSql = "SELECT COUNT(1) FROM Users WHERE RoleId = 1";
        const string insertUserSql = @"
            INSERT INTO Users (Id, Username, Password, Salt, 
            Email, PhoneNumber, FirstName, LastName, 
            Group, Sector, ProfilePictureUrl, 
            RoleId, RegisteredAt, LastLoginAt)
            VALUES (@Id, @Username, @Password, @Salt, 
            @Email, @PhoneNumber, @FirstName, @LastName, 
            @Group, @Sector, @ProfilePictureUrl, 
            @RoleId, @RegisteredAt, @LastLoginAt)";
            
        using var connection = await _connectionFactory.CreateConnectionAsync();
        
        var usernameExists = await connection.ExecuteScalarAsync<int>(checkUsernameSql, new { user.Username });
        if (usernameExists > 0)
        {
            throw new ArgumentException("Username already exists.");
        }
        
        var emailExists = await connection.ExecuteScalarAsync<int>(checkEmailSql, new { user.Email });
        if (emailExists > 0)
        {
            throw new ArgumentException("Email already exists.");
        }
        
        var phoneExists = await connection.ExecuteScalarAsync<int>(checkPhoneSql, new { user.PhoneNumber });
        if (phoneExists > 0)
        {
            throw new ArgumentException("Phone number already exists.");
        }
        
        var adminExists = await connection.ExecuteScalarAsync<int>(checkAdminSql);
        if (adminExists > 0 && user.RoleId == 1)
        {
            throw new ArgumentException("Admin already exists.");
        }
        
        user.Password = Hasher.HashPassword($"{user.Password}{user.Salt}");
        
        if (profilePicture is not null)
        {
            var uploadResult = await _mediaUpload.UploadImageAsync(profilePicture, "user_profiles");
            user.ProfilePictureUrl = uploadResult.SecureUrl.ToString();
        }
        
        await connection.ExecuteAsync(insertUserSql, user);
        
        return user;
    }
    
    #endregion
    
    #region Login

    public async Task<User> LoginUserAsync(string username, string password)
    {
        const string sql = @"
            SELECT u.*, r.*
            FROM Users u
            LEFT JOIN Roles r ON u.RoleId = r.Id
            WHERE u.Username = @Username";
            
        using var connection = await _connectionFactory.CreateConnectionAsync();
        
        var result = await connection.QueryAsync<User, Role, User>(
            sql,
            (user, role) =>
            {
                user.Role = role;
                return user;
            },
            new { Username = username },
            splitOn: "Id"
        );
        
        var user = result.FirstOrDefault();
        if (user is null)
        {
            throw new KeyNotFoundException("Invalid username or password.");
        }
        
        var hashedPassword = Hasher.HashPassword($"{password}{user.Salt}");
        if (hashedPassword != user.Password)
        {
            throw new KeyNotFoundException("Invalid password.");
        }
        
        user.LastLoginAt = DateTime.UtcNow;
        await connection.ExecuteAsync(sql, user);
        
        return user;
    }
    
    #endregion
    
    #region Refresh
    
    public async Task CreateRefreshTokenAsync(RefreshToken refreshToken)
    {
        const string sql = @"
            INSERT INTO RefreshTokens (Id, Token, UserId, ExpiresAt, CreatedAt, UpdatedAt)
            VALUES (@Id, @Token, @UserId, @ExpiresAt, @CreatedAt, @UpdatedAt)";
            
        using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync(sql, refreshToken);
    }
    
    public async Task<RefreshToken> GetRefreshTokenAsync(string token)
    {
        const string sql = @"
            SELECT r.*, u.*, ro.*
            FROM RefreshTokens r
            LEFT JOIN Users u ON r.UserId = u.Id
            LEFT JOIN Roles ro ON u.RoleId = ro.Id
            WHERE r.Token = @Token";
            
        using var connection = await _connectionFactory.CreateConnectionAsync();
        
        var result = await connection.QueryAsync<RefreshToken, User, Role, RefreshToken>(
            sql,
            (refreshToken, user, role) =>
            {
                refreshToken.User = user;
                if (user != null)
                {
                    user.Role = role;
                }
                return refreshToken;
            },
            new { Token = token },
            splitOn: "Id,Id"
        );
    
        var refreshToken = result.FirstOrDefault();
        if (refreshToken is null)
        {
            throw new KeyNotFoundException("Invalid refresh token.");
        }
        return refreshToken;
    }
    
    public async Task RemoveRefreshTokenAsync(string token)
    {
        const string sql = "DELETE FROM RefreshTokens WHERE Token = @Token";
        
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var affectedRows = await connection.ExecuteAsync(sql, new { Token = token });
        
        if (affectedRows == 0)
        {
            throw new KeyNotFoundException("Invalid refresh token.");
        }
    }
    
    #endregion
    
    #region Logout
    
    public async Task RemoveAllRefreshTokensAsync(Guid userId)
    {
        const string sql = "DELETE FROM RefreshTokens WHERE UserId = @UserId";
        
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var affectedRows = await connection.ExecuteAsync(sql, new { UserId = userId });
        
        if (affectedRows == 0)
        {
            throw new KeyNotFoundException("No refresh tokens found for the user.");
        }
    }
    
    #endregion
}