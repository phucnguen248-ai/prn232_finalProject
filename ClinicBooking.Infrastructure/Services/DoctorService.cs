using ClinicBooking.Core.DTOs.Doctor;
using ClinicBooking.Core.Entities;
using ClinicBooking.Core.Enums;
using ClinicBooking.Core.Interfaces;
using ClinicBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicBooking.Infrastructure.Services;

public class DoctorService : IDoctorService
{
    private readonly ApplicationDbContext _context;

    public DoctorService(ApplicationDbContext context)
    {
        _context = context;
    }

    public IQueryable<Doctor> GetQueryable()
    {
        return _context.Doctors
            .Include(d => d.User)
            .Include(d => d.Specialization)
            .AsQueryable();
    }

    public async Task<IEnumerable<DoctorDto>> GetAllAsync()
    {
        return await _context.Doctors
            .Include(d => d.User)
            .Include(d => d.Specialization)
            .Select(d => new DoctorDto
            {
                DoctorId = d.DoctorId,
                UserId = d.UserId,
                FullName = d.User.FullName,
                Email = d.User.Email,
                PhoneNumber = d.User.PhoneNumber,
                SpecializationId = d.SpecializationId,
                SpecializationName = d.Specialization.Name,
                LicenseNumber = d.LicenseNumber,
                YearsOfExperience = d.YearsOfExperience,
                Bio = d.Bio,
                IsActive = d.User.IsActive
            })
            .ToListAsync();
    }

    public async Task<DoctorDto?> GetByIdAsync(int id)
    {
        var d = await _context.Doctors
            .Include(x => x.User)
            .Include(x => x.Specialization)
            .FirstOrDefaultAsync(x => x.DoctorId == id);

        if (d == null) return null;

        return MapToDto(d);
    }

    public async Task<DoctorDto> CreateAsync(CreateDoctorDto dto)
    {
        bool emailExists = await _context.Users.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower());
        if (emailExists)
        {
            throw new InvalidOperationException("Email bác sĩ này đã tồn tại trong hệ thống.");
        }

        var specExists = await _context.Specializations.AnyAsync(s => s.SpecializationId == dto.SpecializationId);
        if (!specExists)
        {
            throw new InvalidOperationException("Chuyên khoa được chọn không tồn tại.");
        }

        var user = new User
        {
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            FullName = dto.FullName,
            PhoneNumber = dto.PhoneNumber,
            Role = UserRole.Doctor.ToString(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var doctor = new Doctor
        {
            UserId = user.UserId,
            SpecializationId = dto.SpecializationId,
            LicenseNumber = dto.LicenseNumber,
            YearsOfExperience = dto.YearsOfExperience,
            Bio = dto.Bio
        };

        await _context.Doctors.AddAsync(doctor);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(doctor.DoctorId))!;
    }

    public async Task<DoctorDto?> UpdateAsync(int id, UpdateDoctorDto dto)
    {
        var doctor = await _context.Doctors
            .Include(d => d.User)
            .Include(d => d.Specialization)
            .FirstOrDefaultAsync(d => d.DoctorId == id);

        if (doctor == null) return null;

        var specExists = await _context.Specializations.AnyAsync(s => s.SpecializationId == dto.SpecializationId);
        if (!specExists)
        {
            throw new InvalidOperationException("Chuyên khoa được chọn không tồn tại.");
        }

        doctor.User.FullName = dto.FullName;
        doctor.User.PhoneNumber = dto.PhoneNumber;
        doctor.User.IsActive = dto.IsActive;

        doctor.SpecializationId = dto.SpecializationId;
        doctor.LicenseNumber = dto.LicenseNumber;
        doctor.YearsOfExperience = dto.YearsOfExperience;
        doctor.Bio = dto.Bio;

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(doctor.DoctorId))!;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var doctor = await _context.Doctors
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.DoctorId == id);

        if (doctor == null) return false;

        // Deactivate user instead of hard delete if doctor has schedules/appointments
        doctor.User.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }

    private static DoctorDto MapToDto(Doctor d)
    {
        return new DoctorDto
        {
            DoctorId = d.DoctorId,
            UserId = d.UserId,
            FullName = d.User.FullName,
            Email = d.User.Email,
            PhoneNumber = d.User.PhoneNumber,
            SpecializationId = d.SpecializationId,
            SpecializationName = d.Specialization?.Name ?? string.Empty,
            LicenseNumber = d.LicenseNumber,
            YearsOfExperience = d.YearsOfExperience,
            Bio = d.Bio,
            IsActive = d.User.IsActive
        };
    }
}
