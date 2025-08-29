using EmployeeInformationManagement.Models;

namespace EmployeeInformationManagement.Interfaces;

public interface IEmployeeService
{
    Task<string> AddEmployeeAsync(Employee employee);
    Task<List<Employee>> GetAllEmployeesAsync();
    Task<string> UpdateEmployeeAsync(Employee employee);
    Task<string> DeleteEmployeeAsync(int employeeId);
    Task<Employee> GetEmployeeByIdAsync(int employeeId);
}