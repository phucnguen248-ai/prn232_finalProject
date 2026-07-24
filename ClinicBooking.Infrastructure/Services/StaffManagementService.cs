using ClinicBooking.Core.DTOs.Staff;
using ClinicBooking.Core.Entities;
using ClinicBooking.Core.Enums;
using ClinicBooking.Core.Interfaces;
using ClinicBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicBooking.Infrastructure.Services;

public class StaffManagementService : IStaffManagementService
{
    private readonly ApplicationDbContext _context;

    public StaffManagementService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<StaffDto>> GetAllAsync()
    {
        return await _context.Staffs
            .Include(s => s.User)
            .Select(s => new StaffDto
            {
                StaffId = s.StaffId,
                UserId = s.UserId,
                FullName = s.User.FullName,
                Email = s.User.Email,
                PhoneNumber = s.User.PhoneNumber,
                Position = s.Position,
                IsActive = s.User.IsActive
            })
            .ToListAsync();
    }

    public async Task<StaffDto> CreateStaffAsync(CreateStaffDto dto)
    {
        bool emailExists = await _context.Users.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower());
        if (emailExists)
        {
            throw new InvalidOperationException("Email này đã được sử dụng trong hệ thống.");
        }

        var user = new User
        {
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            FullName = dto.FullName,
            PhoneNumber = dto.PhoneNumber,
            Role = UserRole.Staff.ToString(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var staff = new Staff
        {
            UserId = user.UserId,
            Position = dto.Position
        };

        await _context.Staffs.AddAsync(staff);
        await _context.SaveChangesAsync();

        return new StaffDto
        {
            StaffId = staff.StaffId,
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Position = staff.Position,
            IsActive = user.IsActive
        };
    }
}
