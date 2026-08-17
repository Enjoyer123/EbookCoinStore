using Microsoft.EntityFrameworkCore;
using EbookCoinWallet.Api.Data;
using EbookCoinWallet.Api.Models;
using EbookCoinWallet.Api.DTOs.Auth;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace EbookCoinWallet.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
       var group = app.MapGroup("/api/auth");

       group.MapPost("/register", async (RegisterRequest request, AppDbContext context) => 
        {
            if(string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                return Results.BadRequest("Email and password are required");
        
            var emailExists = await context.Users.AnyAsync(u => u.Email == request.Email);
            if(emailExists)
                return Results.BadRequest("Email already exists");
           
            var user = new User
            {
                Email = request.Email,
                PasswordHash =BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = UserRole.Customer,
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            return Results.Ok(new {user.Id, user.Email});
        });

        group.MapPost("/login", async (LoginRequest request, AppDbContext context, IConfiguration config, HttpContext httpContext) =>
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Results.Unauthorized();

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var  token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                claims: claims,
                expires: DateTime.Now.AddDays(2),
                signingCredentials: creds    
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            httpContext.Response.Cookies.Append("access_token", tokenString, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,         
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(2)
            });

            return Results.Ok(new { user.Email, user.Role});
        });

        group.MapPost("/logout", (HttpContext httpContext) =>
        {
            httpContext.Response.Cookies.Delete("access_token");
            return Results.Ok(new { message = "Logged out." });
        });
    }
}