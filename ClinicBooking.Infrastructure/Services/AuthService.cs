using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ClinicBooking.Core.DTOs.Auth;
using ClinicBooking.Core.Entities;
using ClinicBooking.Core.Enums;
using ClinicBooking.Core.Interfaces;
using ClinicBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ClinicBooking.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());
        if (user == null || !user.IsActive)
        {
            return null;
        }

        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            return null;
        }

        return GenerateAuthResponse(user);
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        bool emailExists = await _context.Users.AnyAsync(u => u.Email.ToLower() == request.Email.ToLower());
        if (emailExists)
        {
            throw new InvalidOperationException("Email này đã được sử dụng trong hệ thống.");
        }

        var user = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            Role = request.Role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // If registering as Patient, create Patient profile
        if (user.Role == UserRole.Patient.ToString())
        {
            var patient = new Patient
            {
                UserId = user.UserId,
                DateOfBirth = request.DateOfBirth ?? new DateOnly(2000, 1, 1),
                Gender = request.Gender ?? "Khác",
                Address = request.Address ?? string.Empty
            };
            await _context.Patients.AddAsync(patient);
            await _context.SaveChangesAsync();
        }

        return GenerateAuthResponse(user);
    }

    public async Task<UserDto?> GetUserByIdAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return null;

        return new UserDto
        {
            UserId = user.UserId,
            Email = user.Email,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }

    private AuthResponseDto GenerateAuthResponse(User user)
    {
        var secretKey = _configuration["JwtSettings:SecretKey"] ?? "ClinicAppointmentBookingSuperSecretKey2026SecureJwtTokenKey!";
        var issuer = _configuration["JwtSettings:Issuer"] ?? "ClinicBookingApi";
        var audience = _configuration["JwtSettings:Audience"] ?? "ClinicBookingClient";
        var expiryMinutes = int.Parse(_configuration["JwtSettings:ExpiryMinutes"] ?? "120");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var expiration = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiration,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwtTokenString = tokenHandler.WriteToken(token);

        return new AuthResponseDto
        {
            Token = jwtTokenString,
            Expiration = expiration,
            User = new UserDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            }
        };
    }
}
