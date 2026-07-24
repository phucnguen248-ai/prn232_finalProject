using ClinicBooking.Core.DTOs.Specialization;
using ClinicBooking.Core.Entities;

namespace ClinicBooking.Core.Interfaces;

public interface ISpecializationService
{
    IQueryable<Specialization> GetQueryable();
    Task<IEnumerable<SpecializationDto>> GetAllAsync();
    Task<SpecializationDto?> GetByIdAsync(int id);
    Task<SpecializationDto> CreateAsync(CreateSpecializationDto dto);
    Task<SpecializationDto?> UpdateAsync(int id, CreateSpecializationDto dto);
    Task<bool> DeleteAsync(int id);
}
