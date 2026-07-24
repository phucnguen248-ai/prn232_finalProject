using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ClinicBooking.Core.Interfaces;
using ClinicBooking.Core.Validators;
using ClinicBooking.Infrastructure.Data;
using ClinicBooking.Infrastructure.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

// Clear inbound claim type mapping to preserve JWT claim names
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

// 0. Enable CORS for Frontend (Razor Pages AJAX)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 1. Add DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, b => b.MigrationsAssembly("ClinicBooking.Infrastructure")));

// 2. Add Business Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISpecializationService, SpecializationService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IReceptionService, ReceptionService>();
builder.Services.AddScoped<IDoctorDashboardService, DoctorDashboardService>();
builder.Services.AddScoped<IStaffManagementService, StaffManagementService>();
builder.Services.AddScoped<IScheduleBatchService, ScheduleBatchService>();

// 3. Add FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestDtoValidator>();

// 4. Add Authentication & JWT Bearer
var jwtSecretKey = builder.Configuration["JwtSettings:SecretKey"] ?? "ClinicAppointmentBookingSuperSecretKey2026SecureJwtTokenKey!";
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "ClinicBookingApi",
        ValidAudience = builder.Configuration["JwtSettings:Audience"] ?? "ClinicBookingClient",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        RoleClaimType = ClaimTypes.Role,
        NameClaimType = ClaimTypes.NameIdentifier
    };
});

builder.Services.AddAuthorization();

// 5. Add Controllers & OData
builder.Services.AddControllers()
    .AddOData(options => options
        .Select()
        .Filter()
        .OrderBy()
        .Expand()
        .Count()
        .SetMaxTop(100));

builder.Services.AddEndpointsApiExplorer();

// 6. Add Swagger with JWT Bearer Definition
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Clinic Appointment Booking API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Nhập JWT Bearer Token theo định dạng: Bearer {your_token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Patient booking
builder.Services.AddScoped<IBookingService, BookingService>();

var app = builder.Build();

// Enable CORS
app.UseCors("AllowAll");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// NOTE: Disabled UseHttpsRedirection to prevent 307 redirects from stripping Authorization header on HTTP AJAX requests
// app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed Database automatically on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await DbInitializer.SeedAsync(dbContext);
}

app.Run();
