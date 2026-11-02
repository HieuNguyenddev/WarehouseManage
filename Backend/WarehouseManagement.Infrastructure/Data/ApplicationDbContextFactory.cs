
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace WarehouseManagement.Infrastructure.Data;

/// <summary>
/// Factory được sử dụng khi chạy Entity Framework Core CLI.
/// Đọc cấu hình trực tiếp từ appsettings.json của WebAPI.
/// </summary>
public class ApplicationDbContextFactory
    : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    private const string WebApiFolder = "WarehouseManagement.WebAPI";

    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var webApiPath = FindWebApiPath();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(webApiPath)
            .AddJsonFile(
                "appsettings.json",
                optional: false,
                reloadOnChange: false)
            .AddJsonFile(
                "appsettings.Development.json",
                optional: true,
                reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        var connectionString =
            configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Không tìm thấy ConnectionStrings:DefaultConnection " +
                "trong appsettings.json.");
        }

        var optionsBuilder =
            new DbContextOptionsBuilder<ApplicationDbContext>();

        optionsBuilder.UseSqlServer(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }

    private static string FindWebApiPath()
    {
        var currentDirectory = new DirectoryInfo(
            Directory.GetCurrentDirectory());

        while (currentDirectory != null)
        {
            // Trường hợp chạy lệnh tại thư mục Backend
            var candidate = Path.Combine(
                currentDirectory.FullName,
                WebApiFolder);

            if (File.Exists(
                    Path.Combine(candidate, "appsettings.json")))
            {
                return candidate;
            }

            // Trường hợp chạy lệnh ngay trong thư mục WebAPI
            if (currentDirectory.Name == WebApiFolder &&
                File.Exists(Path.Combine(
                    currentDirectory.FullName,
                    "appsettings.json")))
            {
                return currentDirectory.FullName;
            }

            currentDirectory = currentDirectory.Parent;
        }

        throw new DirectoryNotFoundException(
            $"Không tìm thấy thư mục {WebApiFolder} " +
            "hoặc file appsettings.json.");
    }
}
