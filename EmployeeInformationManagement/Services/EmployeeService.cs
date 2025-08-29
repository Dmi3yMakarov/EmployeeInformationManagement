using EmployeeInformationManagement.Context;
using EmployeeInformationManagement.Interfaces;
using EmployeeInformationManagement.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EmployeeInformationManagement.Services;

public class EmployeeService : IEmployeeService
{
    private readonly ApplicationDbContext _context;

    public EmployeeService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> AddEmployeeAsync(Employee employee)
    {
        try
        {
            var parameters = new[]
            {
                new SqlParameter("@FirstName", employee.FirstName),
                new SqlParameter("@LastName", employee.LastName),
                new SqlParameter("@Email", employee.Email),
                new SqlParameter("@DateOfBirth", employee.DateOfBirth ?? (object)DBNull.Value),
                new SqlParameter("@Salary", employee.Salary ?? (object)DBNull.Value)
            };

            var result = await _context.Database.ExecuteSqlRawAsync("EXEC AddEmployee @FirstName, @LastName, @Email, @DateOfBirth, @Salary", parameters);

            return "Сотрудник успешно добавлен";
        }
        catch (SqlException ex)
        {
            return $"Ошибка: {ex.Message}";
        }
        catch (Exception ex)
        {
            return $"Общая ошибка: {ex.Message}";
        }
    }

    public async Task<List<Employee>> GetAllEmployeesAsync()
    {
        try
        {
            var employees = await _context.Employees
                .FromSqlRaw("EXEC GetAllEmployees")
                .AsNoTracking()
                .ToListAsync();

            return employees;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при получении сотрудников: {ex.Message}");
            return new List<Employee>();
        }
    }

    public async Task<string> UpdateEmployeeAsync(Employee employee)
    {
        try
        {
            var parameters = new[]
            {
                new SqlParameter("@EmployeeID", employee.EmployeeID),
                new SqlParameter("@FirstName", employee.FirstName ?? (object)DBNull.Value),
                new SqlParameter("@LastName", employee.LastName ?? (object)DBNull.Value),
                new SqlParameter("@Email", employee.Email ?? (object)DBNull.Value),
                new SqlParameter("@DateOfBirth", employee.DateOfBirth ?? (object)DBNull.Value),
                new SqlParameter("@Salary", employee.Salary ?? (object)DBNull.Value)
            };

            var result = await _context.Database
                .ExecuteSqlRawAsync("EXEC UpdateEmployee @EmployeeID, @FirstName, @LastName, @Email, @DateOfBirth, @Salary", parameters);

            return "Данные сотрудника успешно обновлены";
        }
        catch (SqlException ex)
        {
            return $"Ошибка: {ex.Message}";
        }
    }

    public async Task<string> DeleteEmployeeAsync(int employeeId)
    {
        try
        {
            var parameter = new SqlParameter("@EmployeeID", employeeId);

            var result = await _context.Database
                .ExecuteSqlRawAsync("EXEC DeleteEmployee @EmployeeID", parameter);

            return "Сотрудник успешно удален";
        }
        catch (SqlException ex)
        {
            return $"Ошибка: {ex.Message}";
        }
    }

    public async Task<Employee> GetEmployeeByIdAsync(int employeeId)
    {
        try
        {
            var parameter = new SqlParameter("@EmployeeID", employeeId);

            var employees = await _context.Employees
                .FromSqlRaw("EXEC GetEmployeeById @EmployeeID", parameter)
                .AsNoTracking()
                .ToListAsync();

            return employees.FirstOrDefault();
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Ошибка при получении сотрудника: {ex.Message}");
        }
    }
}