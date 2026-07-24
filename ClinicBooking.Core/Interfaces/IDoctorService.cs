using ClinicBooking.Core.DTOs.Doctor;
using ClinicBooking.Core.Entities;

namespace ClinicBooking.Core.Interfaces;

public interface IDoctorService
{
    IQueryable<Doctor> GetQueryable();
    Task<IEnumerable<DoctorDto>> GetAllAsync();
    Task<DoctorDto?> GetByIdAsync(int id);
    Task<DoctorDto> CreateAsync(CreateDoctorDto dto);
    Task<DoctorDto?> UpdateAsync(int id, UpdateDoctorDto dto);
    Task<bool> DeleteAsync(int id);
}
