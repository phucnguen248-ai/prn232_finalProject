using ClinicBooking.Core.Entities;
using ClinicBooking.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace ClinicBooking.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        await context.Database.MigrateAsync();

        if (await context.Users.AnyAsync())
        {
            return; // DB has been seeded
        }

        // 1. Seed Specializations
        var specs = new List<Specialization>
        {
            new Specialization { Name = "Nội Khoa", Description = "Khám và điều trị các bệnh lý nội khoa tổng quát" },
            new Specialization { Name = "Nhi Khoa", Description = "Khám và tư vấn sức khỏe trẻ em" },
            new Specialization { Name = "Tai Mũi Họng", Description = "Chẩn đoán và điều trị bệnh lý Tai Mũi Họng" },
            new Specialization { Name = "Da Liễu", Description = "Chăm sóc và điều trị các vấn đề bệnh lý về da" }
        };
        await context.Specializations.AddRangeAsync(specs);
        await context.SaveChangesAsync();

        // 2. Seed Admin User
        var adminUser = new User
        {
            Email = "admin@clinic.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            FullName = "Hệ Thống Admin",
            PhoneNumber = "0900000000",
            Role = UserRole.Admin.ToString(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await context.Users.AddAsync(adminUser);

        // 3. Seed Staff User & Staff Profile
        var staffUser = new User
        {
            Email = "staff@clinic.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Staff123!"),
            FullName = "Lễ Tân Nguyễn Thị Mai",
            PhoneNumber = "0911111111",
            Role = UserRole.Staff.ToString(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await context.Users.AddAsync(staffUser);
        await context.SaveChangesAsync();

        var staffProfile = new Staff
        {
            UserId = staffUser.UserId,
            Position = "Trưởng ca lễ tân"
        };
        await context.Staffs.AddAsync(staffProfile);

        // 4. Seed Doctor Users & Profiles
        var doc1User = new User
        {
            Email = "doctor1@clinic.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Doctor123!"),
            FullName = "BS. Nguyễn Văn An",
            PhoneNumber = "0922222222",
            Role = UserRole.Doctor.ToString(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        var doc2User = new User
        {
            Email = "doctor2@clinic.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Doctor123!"),
            FullName = "BS. Trần Thị Bình",
            PhoneNumber = "0933333333",
            Role = UserRole.Doctor.ToString(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await context.Users.AddRangeAsync(doc1User, doc2User);
        await context.SaveChangesAsync();

        var doc1Profile = new Doctor
        {
            UserId = doc1User.UserId,
            SpecializationId = specs[0].SpecializationId, // Nội khoa
            LicenseNumber = "MED-LIC-001",
            YearsOfExperience = 12,
            Bio = "Chuyên gia về tim mạch và nội khoa tổng quát với 12 năm kinh nghiệm."
        };
        var doc2Profile = new Doctor
        {
            UserId = doc2User.UserId,
            SpecializationId = specs[1].SpecializationId, // Nhi khoa
            LicenseNumber = "MED-LIC-002",
            YearsOfExperience = 8,
            Bio = "Bác sĩ chuyên khoa Nhi tận tâm, giàu kinh nghiệm chăm sóc sức khỏe trẻ nhỏ."
        };
        await context.Doctors.AddRangeAsync(doc1Profile, doc2Profile);

        // 5. Seed Patient User & Profile
        var patientUser = new User
        {
            Email = "patient1@clinic.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Patient123!"),
            FullName = "Bệnh Nhân Lê Văn Cường",
            PhoneNumber = "0944444444",
            Role = UserRole.Patient.ToString(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await context.Users.AddAsync(patientUser);
        await context.SaveChangesAsync();

        var patientProfile = new Patient
        {
            UserId = patientUser.UserId,
            DateOfBirth = new DateOnly(1992, 8, 20),
            Gender = "Nam",
            Address = "123 Đường Lê Lợi, Phường Bến Thành, Quận 1, TP.HCM"
        };
        await context.Patients.AddAsync(patientProfile);
        await context.SaveChangesAsync();

        // 6. Seed Schedules for Doctor 1 & 2
        var today = DateOnly.FromDateTime(DateTime.Today);
        var tomorrow = today.AddDays(1);

        var schedules = new List<Schedule>
        {
            // Today Slots for Doctor 1
            new Schedule
            {
                DoctorId = doc1Profile.DoctorId,
                SlotDate = today,
                StartTime = new TimeOnly(8, 0),
                EndTime = new TimeOnly(9, 0),
                Status = ScheduleStatus.Booked.ToString(),
                CreatedAt = DateTime.UtcNow
            },
            new Schedule
            {
                DoctorId = doc1Profile.DoctorId,
                SlotDate = today,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(10, 0),
                Status = ScheduleStatus.Available.ToString(),
                CreatedAt = DateTime.UtcNow
            },
            new Schedule
            {
                DoctorId = doc1Profile.DoctorId,
                SlotDate = today,
                StartTime = new TimeOnly(10, 0),
                EndTime = new TimeOnly(11, 0),
                Status = ScheduleStatus.Available.ToString(),
                CreatedAt = DateTime.UtcNow
            },
            // Tomorrow Slots for Doctor 2
            new Schedule
            {
                DoctorId = doc2Profile.DoctorId,
                SlotDate = tomorrow,
                StartTime = new TimeOnly(14, 0),
                EndTime = new TimeOnly(15, 0),
                Status = ScheduleStatus.Available.ToString(),
                CreatedAt = DateTime.UtcNow
            },
            new Schedule
            {
                DoctorId = doc2Profile.DoctorId,
                SlotDate = tomorrow,
                StartTime = new TimeOnly(15, 0),
                EndTime = new TimeOnly(16, 0),
                Status = ScheduleStatus.Available.ToString(),
                CreatedAt = DateTime.UtcNow
            }
        };
        await context.Schedules.AddRangeAsync(schedules);
        await context.SaveChangesAsync();

        // 7. Seed Sample Appointment for the Booked Schedule
        var sampleAppointment = new Appointment
        {
            PatientId = patientProfile.PatientId,
            DoctorId = doc1Profile.DoctorId,
            ScheduleId = schedules[0].ScheduleId,
            Status = AppointmentStatus.Confirmed.ToString(),
            Reason = "Tái khám định kỳ sức khỏe tim mạch và theo dõi huyết áp.",
            CreatedAt = DateTime.UtcNow
        };
        await context.Appointments.AddAsync(sampleAppointment);
        await context.SaveChangesAsync();
    }
}
