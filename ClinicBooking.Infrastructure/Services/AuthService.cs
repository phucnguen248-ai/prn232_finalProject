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

    public async Task<UserProfileDto?> GetUserProfileAsync(int userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (user == null) return null;

        var dto = new UserProfileDto
        {
            UserId = user.UserId,
            Email = user.Email,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role
        };

        if (user.Role == UserRole.Patient.ToString())
        {
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
            if (patient != null)
            {
                dto.DateOfBirth = patient.DateOfBirth;
                dto.Gender = patient.Gender;
                dto.Address = patient.Address;
            }
        }
        else if (user.Role == UserRole.Doctor.ToString())
        {
            var doctor = await _context.Doctors
                .Include(d => d.Specialization)
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor != null)
            {
                dto.SpecializationName = doctor.Specialization?.Name ?? string.Empty;
                dto.LicenseNumber = doctor.LicenseNumber;
                dto.YearsOfExperience = doctor.YearsOfExperience;
                dto.Bio = doctor.Bio;
            }
        }
        else if (user.Role == UserRole.Staff.ToString())
        {
            var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.UserId == userId);
            if (staff != null)
            {
                dto.Position = staff.Position;
            }
        }

        return dto;
    }

    public async Task<UserProfileDto> UpdateUserProfileAsync(int userId, UpdateProfileDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (user == null)
        {
            throw new InvalidOperationException("Không tìm thấy thông tin tài khoản.");
        }

        // Update User info
        user.FullName = dto.FullName;
        user.PhoneNumber = dto.PhoneNumber;

        // Change password if provided
        if (!string.IsNullOrEmpty(dto.NewPassword))
        {
            if (string.IsNullOrEmpty(dto.CurrentPassword) || !BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            {
                throw new InvalidOperationException("Mật khẩu hiện tại không chính xác.");
            }

            if (dto.NewPassword.Length < 6)
            {
                throw new InvalidOperationException("Mật khẩu mới phải từ 6 ký tự trở lên.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        }

        // Update role-specific profile
        if (user.Role == UserRole.Patient.ToString())
        {
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
            if (patient != null)
            {
                if (dto.DateOfBirth.HasValue) patient.DateOfBirth = dto.DateOfBirth.Value;
                if (!string.IsNullOrEmpty(dto.Gender)) patient.Gender = dto.Gender;
                if (dto.Address != null) patient.Address = dto.Address;
            }
        }
        else if (user.Role == UserRole.Doctor.ToString())
        {
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
            if (doctor != null)
            {
                if (dto.YearsOfExperience.HasValue) doctor.YearsOfExperience = dto.YearsOfExperience.Value;
                if (dto.Bio != null) doctor.Bio = dto.Bio;
            }
        }
        else if (user.Role == UserRole.Staff.ToString())
        {
            var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.UserId == userId);
            if (staff != null)
            {
                if (!string.IsNullOrEmpty(dto.Position)) staff.Position = dto.Position;
            }
        }

        await _context.SaveChangesAsync();

        return (await GetUserProfileAsync(userId))!;
    }

    public async Task<bool> ForgotPasswordResetAsync(ForgotPasswordDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => 
            u.Email.ToLower() == dto.Email.Trim().ToLower() && 
            u.PhoneNumber.Trim() == dto.PhoneNumber.Trim());

        if (user == null || !user.IsActive)
        {
            throw new InvalidOperationException("Thông tin Email hoặc Số điện thoại không xác thực được.");
        }

        if (string.IsNullOrEmpty(dto.NewPassword) || dto.NewPassword.Length < 6)
        {
            throw new InvalidOperationException("Mật khẩu mới phải từ 6 ký tự trở lên.");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _context.SaveChangesAsync();
        return true;
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
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("role", user.Role)
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
