using ClinicBooking.Core.DTOs.Staff;

namespace ClinicBooking.Core.Interfaces;

public interface IStaffManagementService
{
    Task<IEnumerable<StaffDto>> GetAllAsync();
    Task<StaffDto> CreateStaffAsync(CreateStaffDto dto);
}
