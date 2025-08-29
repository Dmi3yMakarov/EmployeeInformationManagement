using EmployeeInformationManagement.Context;
using EmployeeInformationManagement.Interfaces;
using EmployeeInformationManagement.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeInformationManagement;

internal class Program
{
    static async Task Main(string[] args)
    {
        // Загружаем конфигурацию
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // Настраиваем сервисы
        var services = new ServiceCollection();

        // Регистрируем DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Регистрируем сервисы
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<Application>();

        // Создаем провайдер сервисов
        var serviceProvider = services.BuildServiceProvider();

        // Запускаем приложение
        using var scope = serviceProvider.CreateScope();
        var app = scope.ServiceProvider.GetRequiredService<Application>();
        await app.RunAsync();
    }
}
