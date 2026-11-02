
using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

using WarehouseManagement.Application.Interfaces;
using WarehouseManagement.Application.Services;

using WarehouseManagement.Infrastructure.Data;
using WarehouseManagement.Infrastructure.Repositories;
using WarehouseManagement.Infrastructure.Services;
using WarehouseManagement.Infrastructure.Settings;

var builder = WebApplication.CreateBuilder(args);

// 1. LOAD CONFIGURATION

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Thiếu ConnectionStrings:DefaultConnection trong appsettings.json.");
}

// 2. REGISTER SERVICES

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

// Swagger + JWT Bearer Authentication
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập JWT Token (không cần thêm tiền tố Bearer)."
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });
});

builder.Services.AddProblemDetails();

// 3. DATABASE CONFIGURATION

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(
        connectionString,
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure();
        });
});

// 4. ROLE MODULE

builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IRoleService, RoleService>();

// 5. USER MODULE

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();

// 5.1 FILE MODULE

builder.Services.AddScoped<IFileRepository, FileRepository>();
builder.Services.AddScoped<IFileService, FileService>();

// 6. AUTH MODULE

builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

// 7. JWT CONFIGURATION

var jwt = builder.Configuration
    .GetSection(JwtSettings.SectionName)
    .Get<JwtSettings>();

if (jwt == null)
{
    throw new InvalidOperationException(
        "Không tìm thấy cấu hình Jwt trong appsettings.json.");
}

if (string.IsNullOrWhiteSpace(jwt.Key) || jwt.Key.Length < 32)
{
    throw new InvalidOperationException(
        "Jwt:Key phải có ít nhất 32 ký tự.");
}

if (string.IsNullOrWhiteSpace(jwt.Issuer))
{
    throw new InvalidOperationException(
        "Thiếu Jwt:Issuer.");
}

if (string.IsNullOrWhiteSpace(jwt.Audience))
{
    throw new InvalidOperationException(
        "Thiếu Jwt:Audience.");
}

// Đăng ký JwtSettings
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection(JwtSettings.SectionName));

// Authentication
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,

            ValidateAudience = true,
            ValidAudience = jwt.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt.Key)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,

            NameClaimType = "name",
            RoleClaimType = "role"
        };
    });

// Authorization
builder.Services.AddAuthorization();

// 8. BUILD APPLICATION

var app = builder.Build();

// 9. DATABASE MIGRATION & SEEDING

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var db = services.GetRequiredService<ApplicationDbContext>();

        // Apply database migrations
        await db.Database.MigrateAsync();

        // Seed initial roles and admin account
        await DbSeeder.SeedAsync(services);
    }
    catch (Exception exception)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();

        logger.LogError(
            exception,
            "Lỗi khi Migration hoặc Seed dữ liệu Database.");

        throw;
    }
}

// 10. HTTP REQUEST PIPELINE

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler();
    app.UseHttpsRedirection();
}

// Authentication phải đứng trước Authorization
app.UseAuthentication();

app.UseAuthorization();

// Map Controllers
app.MapControllers();

// 11. RUN APPLICATION

app.Run();
