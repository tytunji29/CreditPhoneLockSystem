using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Domain;
using Infrastructure;
using Infrastructure.Data;
using Infrastructure.Data.AppDbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

public interface IAdminService
{
    Task<ReturnObject> RegisterAsync(string fullName, string email, string password, bool isSuperAdmin);
    Task<ReturnObject> LoginAsync(string email, string password);
}

public class AdminService : IAdminService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _config;

    public AdminService(IUnitOfWork unitOfWork, IConfiguration config)
    {
        _unitOfWork = unitOfWork;
        _config = config;
    }

    public async Task<ReturnObject> RegisterAsync(string fullName, string email, string password, bool isSuperAdmin)
    {
        try
        {
            // Check if email already exists
            if (await _unitOfWork.AdminUsers.AnyAsync(u => u.Email == email))
                throw new Exception("Email already exists");

            // Create Salt
            using var hmac = new HMACSHA512();
            var salt = Convert.ToBase64String(hmac.Key);

            // Hash password with salt
            var hash = Convert.ToBase64String(
                hmac.ComputeHash(Encoding.UTF8.GetBytes(password))
            );
            string rolename = isSuperAdmin ? "SuperAdmin" : "Admin";
            var existingRole = await _unitOfWork.Roles.GetByPropertyAsync(u => u.Name == rolename);
            Role role;
            if (existingRole == null)
            {
                role = new Role
                {
                    Id = Guid.NewGuid(),
                    Name = rolename,
                    Description = $"This Is For {rolename}"
                };
                await _unitOfWork.Roles.AddAsync(role);
            }
            else
            {
                role = existingRole;
            }
            var user = new AdminUser
            {
                Id = Guid.NewGuid(),
                FullName = fullName,
                IsSuperAdmin = isSuperAdmin,
                Email = email,
                PasswordSalt = salt,
                PasswordHash = hash,
                RoleId = role.Id
            };
            await _unitOfWork.AdminUsers.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            string token = GenerateJwtToken(user);

            return new ReturnObject
            {
                Status = true,
                Message = "Admin registered successfully",
                Data = new
                {
                    user.Id,
                    user.FullName,
                    user.Email,
                    Token = token,
                    user.Role
                }
            };
        }
        catch (Exception ex)
        {
            await FileLogger.WriteLogAsync(ex.InnerException?.Message ?? ex.Message);
            return new ReturnObject
            {
                Status = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ReturnObject> LoginAsync(string email, string password)
    {
        try
        {
            var user = await _unitOfWork.AdminUsers
                .GetByPropertyAsync(u => u.Email == email);

            if (user == null)
                return new ReturnObject
                {
                    Status = false,
                    Message = "Invalid email or password"
                };

            // Verify password
            var saltBytes = Convert.FromBase64String(user.PasswordSalt);
            using var hmac = new HMACSHA512(saltBytes);
            var hash = Convert.ToBase64String(
                hmac.ComputeHash(Encoding.UTF8.GetBytes(password))
            );

            if (hash != user.PasswordHash)
                return new ReturnObject
                {
                    Status = false,
                    Message = "Invalid email or password"
                };

            string token = GenerateJwtToken(user);

            return new ReturnObject
            {
                Status = true,
                Message = "Login successful",
                Data = new
                {
                    user.Id,
                    user.FullName,
                    user.Email,
                    Token = token,
                    user.Role
                }
            };
        }
        catch (Exception ex)
        {
            await FileLogger.WriteLogAsync(ex.InnerException?.Message ?? ex.Message);
            return new ReturnObject
            {
                Status = false,
                Message = ex.Message
            };
        }
    }

    private string GenerateJwtToken(AdminUser user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("fullname", user.FullName),
            new Claim("userId", user.Id.ToString())
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"])
        );

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
