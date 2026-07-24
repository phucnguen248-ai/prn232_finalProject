using ClinicBooking.Core.DTOs.Specialization;
using ClinicBooking.Core.Entities;
using ClinicBooking.Core.Interfaces;
using ClinicBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicBooking.Infrastructure.Services;

public class SpecializationService : ISpecializationService
{
    private readonly ApplicationDbContext _context;

    public SpecializationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public IQueryable<Specialization> GetQueryable()
    {
        return _context.Specializations.AsQueryable();
    }

    public async Task<IEnumerable<SpecializationDto>> GetAllAsync()
    {
        return await _context.Specializations
            .Select(s => new SpecializationDto
            {
                SpecializationId = s.SpecializationId,
                Name = s.Name,
                Description = s.Description,
                DoctorCount = s.Doctors.Count
            })
            .ToListAsync();
    }

    public async Task<SpecializationDto?> GetByIdAsync(int id)
    {
        var s = await _context.Specializations
            .Include(x => x.Doctors)
            .FirstOrDefaultAsync(x => x.SpecializationId == id);

        if (s == null) return null;

        return new SpecializationDto
        {
            SpecializationId = s.SpecializationId,
            Name = s.Name,
            Description = s.Description,
            DoctorCount = s.Doctors.Count
        };
    }

    public async Task<SpecializationDto> CreateAsync(CreateSpecializationDto dto)
    {
        var spec = new Specialization
        {
            Name = dto.Name,
            Description = dto.Description
        };

        await _context.Specializations.AddAsync(spec);
        await _context.SaveChangesAsync();

        return new SpecializationDto
        {
            SpecializationId = spec.SpecializationId,
            Name = spec.Name,
            Description = spec.Description,
            DoctorCount = 0
        };
    }

    public async Task<SpecializationDto?> UpdateAsync(int id, CreateSpecializationDto dto)
    {
        var spec = await _context.Specializations.FindAsync(id);
        if (spec == null) return null;

        spec.Name = dto.Name;
        spec.Description = dto.Description;

        await _context.SaveChangesAsync();

        return new SpecializationDto
        {
            SpecializationId = spec.SpecializationId,
            Name = spec.Name,
            Description = spec.Description,
            DoctorCount = await _context.Doctors.CountAsync(d => d.SpecializationId == id)
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var spec = await _context.Specializations
            .Include(s => s.Doctors)
            .FirstOrDefaultAsync(s => s.SpecializationId == id);

        if (spec == null) return false;

        if (spec.Doctors.Any())
        {
            throw new InvalidOperationException("Không thể xóa chuyên khoa đang có bác sĩ hoạt động.");
        }

        _context.Specializations.Remove(spec);
        await _context.SaveChangesAsync();
        return true;
    }
}
