using ClinicBooking.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicBooking.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Staff> Staffs => Set<Staff>();
    public DbSet<Specialization> Specializations => Set<Specialization>();
    public DbSet<Schedule> Schedules => Set<Schedule>();
    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. User Entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.UserId);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Email).IsRequired().HasMaxLength(150);
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.FullName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.PhoneNumber).HasMaxLength(20);
            entity.Property(u => u.Role).IsRequired().HasMaxLength(20);
            entity.Property(u => u.IsActive).HasDefaultValue(true);
            entity.Property(u => u.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // 2. Patient Entity (1:1 with User)
        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(p => p.PatientId);
            entity.HasIndex(p => p.UserId).IsUnique();

            entity.HasOne(p => p.User)
                .WithOne(u => u.Patient)
                .HasForeignKey<Patient>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(p => p.Gender).HasMaxLength(10);
            entity.Property(p => p.Address).HasMaxLength(500);
        });

        // 3. Doctor Entity (1:1 with User, N:1 with Specialization)
        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.HasKey(d => d.DoctorId);
            entity.HasIndex(d => d.UserId).IsUnique();

            entity.HasOne(d => d.User)
                .WithOne(u => u.Doctor)
                .HasForeignKey<Doctor>(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Specialization)
                .WithMany(s => s.Doctors)
                .HasForeignKey(d => d.SpecializationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(d => d.LicenseNumber).IsRequired().HasMaxLength(50);
            entity.Property(d => d.Bio).HasMaxLength(1000);
        });

        // 4. Staff Entity (1:1 with User)
        modelBuilder.Entity<Staff>(entity =>
        {
            entity.HasKey(s => s.StaffId);
            entity.HasIndex(s => s.UserId).IsUnique();

            entity.HasOne(s => s.User)
                .WithOne(u => u.Staff)
                .HasForeignKey<Staff>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(s => s.Position).HasMaxLength(100);
        });

        // 5. Specialization Entity
        modelBuilder.Entity<Specialization>(entity =>
        {
            entity.HasKey(s => s.SpecializationId);
            entity.Property(s => s.Name).IsRequired().HasMaxLength(100);
            entity.Property(s => s.Description).HasMaxLength(500);
        });

        // 6. Schedule Entity (N:1 with Doctor)
        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.HasKey(s => s.ScheduleId);

            entity.HasOne(s => s.Doctor)
                .WithMany(d => d.Schedules)
                .HasForeignKey(s => s.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(s => s.Status).IsRequired().HasMaxLength(30);
            entity.Property(s => s.CancelReason).HasMaxLength(500);
            entity.Property(s => s.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // 7. Appointment Entity (N:1 with Patient, N:1 with Doctor, UNIQUE with Schedule for active non-cancelled appointments)
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(a => a.AppointmentId);

            // Filtered Unique Index: Chỉ duy nhất với các cuộc hẹn KHÁC Cancelled
            entity.HasIndex(a => a.ScheduleId)
                .IsUnique()
                .HasFilter("[Status] <> 'Cancelled'");

            entity.HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.Schedule)
                .WithOne(s => s.Appointment)
                .HasForeignKey<Appointment>(a => a.ScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.Staff)
                .WithMany(s => s.Appointments)
                .HasForeignKey(a => a.StaffId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(a => a.Status).IsRequired().HasMaxLength(20);
            entity.Property(a => a.Reason).IsRequired().HasMaxLength(500);
            entity.Property(a => a.Notes).HasMaxLength(1000);
            entity.Property(a => a.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });
    }
}
