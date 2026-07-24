using ClinicBooking.Core.DTOs;
using ClinicBooking.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DoctorsController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public DoctorsController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DoctorSummaryDto>>> GetDoctors(
        [FromQuery] string? name,
        [FromQuery] string? specialization)
    {
        var doctors = _dbContext.Doctors
            .AsNoTracking()
            .Where(d => d.User.IsActive);

        if (!string.IsNullOrWhiteSpace(name))
        {
            var normalizedName = name.Trim();
            doctors = doctors.Where(d => d.User.FullName.Contains(normalizedName));
        }

        if (!string.IsNullOrWhiteSpace(specialization))
        {
            var normalizedSpecialization = specialization.Trim();
            doctors = doctors.Where(d => d.Specialization.Name.Contains(normalizedSpecialization));
        }

        var result = await doctors
            .OrderBy(d => d.User.FullName)
            .Select(d => new DoctorSummaryDto
            {
                DoctorId = d.DoctorId,
                FullName = d.User.FullName,
                SpecializationName = d.Specialization.Name,
                LicenseNumber = d.LicenseNumber,
                YearsOfExperience = d.YearsOfExperience,
                Bio = d.Bio
            })
            .ToListAsync();

        return Ok(result);
    }
}
