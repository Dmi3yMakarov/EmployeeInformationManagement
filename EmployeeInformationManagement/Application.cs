using EmployeeInformationManagement.Interfaces;
using EmployeeInformationManagement.Models;
using System.Text.RegularExpressions;

namespace EmployeeInformationManagement;

public class Application
{
    private readonly IEmployeeService _employeeService;
    private delegate void writeField(Employee employee);
    private readonly Dictionary<int, writeField> _dictWriteMethods = new Dictionary<int, writeField>();

    public Application(IEmployeeService employeeService)
    {
        _employeeService = employeeService;

        _dictWriteMethods.Add(1, new writeField(ReadFirstName));
        _dictWriteMethods.Add(2, new writeField(ReadLastName));
        _dictWriteMethods.Add(3, new writeField(ReadEmail));
        _dictWriteMethods.Add(4, new writeField(ReadDateOfBirth));
        _dictWriteMethods.Add(5, new writeField(ReadSalary));
    }

    public async Task RunAsync()
    {
        Console.WriteLine("Консольное приложение для работы с сотрудниками");
        Console.WriteLine("==============================================");

        while (true)
        {
            Console.WriteLine("\nВыберите действие:");
            Console.WriteLine("1. Добавить сотрудника");
            Console.WriteLine("2. Получить всех сотрудников");
            Console.WriteLine("3. Обновить сотрудника");
            Console.WriteLine("4. Удалить сотрудника");
            Console.WriteLine("5. Выход");

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await AddEmployeeAsync();
                    break;
                case "2":
                    await GetAllEmployeesAsync();
                    break;
                case "3":
                    await UpdateEmployeeAsync();
                    break;
                case "4":
                    await DeleteEmployeeAsync();
                    break;
                case "5":
                    Console.WriteLine("Выход из приложения...");
                    return;
                default:
                    Console.WriteLine("Неверный выбор. Попробуйте снова.");
                    break;
            }
        }
    }

    private async Task AddEmployeeAsync()
    {
        Console.WriteLine("\n=== ДОБАВЛЕНИЕ НОВОГО СОТРУДНИКА ===");

        try
        {
            var employee = new Employee();

            // Ввод имени
            ReadFirstName(employee);

            // Ввод фамилии
            ReadLastName(employee);

            // Ввод email
            ReadEmail(employee);

            // Ввод даты рождения
            ReadDateOfBirth(employee);

            // Ввод зарплаты
            ReadSalary(employee);

            // Подтверждение данных
            Console.WriteLine("\nПроверьте введенные данные:");
            Console.WriteLine($"Имя: {employee.FirstName}");
            Console.WriteLine($"Фамилия: {employee.LastName}");
            Console.WriteLine($"Email: {employee.Email}");
            Console.WriteLine($"Дата рождения: {(employee.DateOfBirth.HasValue ? employee.DateOfBirth.Value.ToString("dd.MM.yyyy") : "не указана")}");
            Console.WriteLine($"Зарплата: {(employee.Salary.HasValue ? employee.Salary.Value.ToString("C") : "не указана")}");

            Console.Write("\nДобавить сотрудника? (y/n): ");
            var confirm = Console.ReadLine();

            if (confirm?.ToLower() == "y")
            {
                var result = await _employeeService.AddEmployeeAsync(employee);
                Console.WriteLine($"\n {result}");
            }
            else
            {
                Console.WriteLine("Добавление отменено");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при добавлении сотрудника: {ex.Message}");
        }
    }

    private async Task GetAllEmployeesAsync()
    {
        try
        {
            var employees = await _employeeService.GetAllEmployeesAsync();
            Console.WriteLine($"\nНайдено сотрудников: {employees.Count}");

            if (employees.Count > 0)
            {
                foreach (var emp in employees)
                {
                    Console.WriteLine($"#{emp.EmployeeID}: {emp.FirstName} {emp.LastName} - {emp.Email} - {emp.Salary}");
                }
            }
            else
            {
                Console.WriteLine("В базе данных нет сотрудников");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }
    
    private async Task UpdateEmployeeAsync()
    {
        Console.WriteLine("\n=== ОБНОВЛЕНИЕ ИНФОРМАЦИИ О СОТРУДНИКЕ ===");

        try
        {
            // Запрос ID сотрудника
            Console.Write("Введите ID сотрудника для обновления: ");
            if (!int.TryParse(Console.ReadLine(), out int employeeId) || employeeId <= 0)
            {
                Console.WriteLine("Неверный формат ID.");
                return;
            }

            // Получение текущих данных сотрудника
            var existingEmployee = await _employeeService.GetEmployeeByIdAsync(employeeId);
            if (existingEmployee == null)
            {
                Console.WriteLine($"Сотрудник с ID {employeeId} не найден.");
                return;
            }

            // Показ текущих данных
            Console.WriteLine("\nТекущие данные сотрудника:");
            DisplayEmployee(existingEmployee);

            // Создание объекта для обновления
            var updatedEmployee = new Employee 
            { 
                EmployeeID = existingEmployee.EmployeeID,
                FirstName = existingEmployee.FirstName,
                LastName = existingEmployee.LastName,
                Email = existingEmployee.Email,
                DateOfBirth = existingEmployee.DateOfBirth,
                Salary = existingEmployee.Salary
            };

            // Меню выбора полей для обновления
            var fieldsToUpdate = SelectFieldsToUpdate();

            GetNewEmployeeData(updatedEmployee, fieldsToUpdate);
                        
            // Подтверждение
            Console.Write("\nПодтвердить обновление? (y/n): ");
            var confirm = Console.ReadLine();

            if (confirm?.ToLower() == "y")
            {
                var result = await _employeeService.UpdateEmployeeAsync(updatedEmployee);
                Console.WriteLine($"\n{result}");
            }
            else
            {
                Console.WriteLine("Обновление отменено");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при обновлении сотрудника: {ex.Message}");
        }
    }

    private List<int> SelectFieldsToUpdate()
    {
        Console.WriteLine("\nВыберите поля для обновления (введите номера через запятую):");
        Console.WriteLine("1. Имя");
        Console.WriteLine("2. Фамилия");
        Console.WriteLine("3. Email");
        Console.WriteLine("4. Дата рождения");
        Console.WriteLine("5. Зарплата");
        Console.WriteLine("6. Все поля");        

        while (true)
        {
            Console.Write("Введите ваш выбор: ");
            var input = Console.ReadLine();

            var selectedNumbers = GetSelectedNumbers(input);

            if(selectedNumbers is null)
            {
                continue;
            }

            return selectedNumbers;
        }
    }

    private List<int> GetSelectedNumbers(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            Console.WriteLine("Необходимо выбрать хотя бы одно поле");
            return null;
        }

        var commands = input.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var selectedNumbers = new List<int>();
        foreach (var s in commands)
        {
            if (!Int32.TryParse(s, out int number))
            {
                Console.WriteLine("Некорректный ввод");
                selectedNumbers.Clear();
                return null;
            }

            if (number < 1 || number > 6)
            {
                Console.WriteLine("Необходимо вводить только цифры от 1 до 6");
                selectedNumbers.Clear();
                return null;
            }

            selectedNumbers.Add(number);
        }
        // здесь починить
        return selectedNumbers;
    }

    private void GetNewEmployeeData(Employee employee, List<int> selectedFields)
    {
        if (selectedFields.Contains(6))
        {
            for (int i = 0; i < 6; i++)
            {
                _dictWriteMethods[i](employee);
            }
        }
        else
        {
            foreach (var fieldNumber in selectedFields)
            {
                _dictWriteMethods[fieldNumber](employee);
            }
        }
    }

    private void DisplayEmployee(Employee employee)
    {
        Console.WriteLine($"ID: {employee.EmployeeID}");
        Console.WriteLine($"Имя: {employee.FirstName}");
        Console.WriteLine($"Фамилия: {employee.LastName}");
        Console.WriteLine($"Email: {employee.Email}");
        Console.WriteLine($"Дата рождения: {(employee.DateOfBirth.HasValue ? employee.DateOfBirth.Value.ToString("dd.MM.yyyy") : "не указана")}");
        Console.WriteLine($"Зарплата: {(employee.Salary.HasValue ? employee.Salary.Value.ToString("C") : "не указана")}");
    }
        
    private async Task DeleteEmployeeAsync()
    {
        Console.WriteLine("Введите ID удаляемого сотрудника: ");
        if (int.TryParse(Console.ReadLine(), out int id))
        {
            var result = await _employeeService.DeleteEmployeeAsync(id);
            Console.WriteLine($"\n {result}");
        }
        else
        {
            Console.WriteLine("Неверный формат ID.");
        }
    }

    private void ReadFirstName(Employee employee)
    {
        Console.Write("Введите имя: ");
        while (true)
        {
            var input = Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(input))
            {
                employee.FirstName =  input;
                return;
            }
            Console.WriteLine("Имя не может быть пустым");
            Console.Write("Попробуйте снова: ");
        }
    }

    private void ReadLastName(Employee employee)
    {
        Console.Write("Введите фамилию: ");
        while (true)
        {
            var input = Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(input))
            {
                employee.LastName = input;
                return;
            }
            Console.WriteLine("Имя не может быть пустым");
            Console.Write("Попробуйте снова: ");
        }
    }

    private void ReadEmail(Employee employee)
    {
        while (true)
        {
            Console.Write("Введите email: ");
            var email = Console.ReadLine()?.Trim();

            if (!IsValidEmail(email))
            {
                Console.WriteLine("Неверный формат email. Пример: user@example.com");
                continue;
            }

            employee.Email = email;
            break;
        }
    }

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        string pattern = @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$";

        return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
    }

    private void ReadDateOfBirth(Employee employee)
    {
        while (true)
        {
            Console.Write("Введите дату рождения (дд.мм.гггг или Enter чтобы пропустить): ");
            var input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input))
            {
                employee.DateOfBirth = null;
                return;
            }

            if (DateTime.TryParseExact(input, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out var date))
            {
                // Проверка что дата не в будущем
                if (date > DateTime.Today)
                {
                    Console.WriteLine("Дата рождения не может быть в будущем");
                    Console.Write("Введите дату рождения: ");
                    continue;
                }

                // Проверка что возраст в диапозоне от 18 до 100 лет
                var age = DateTime.Today.Year - date.Year;
                if (DateTime.Today < date.AddYears(age)) age--; // Корректировка если день рождения еще не наступил

                if (age < 18)
                {
                    Console.WriteLine("Сотрудник должен быть старше 18 лет");
                    Console.Write("Введите дату рождения: ");
                    continue;
                }

                if (age > 100)
                {
                    Console.WriteLine("Возраст слишком большой");
                    Console.Write("Введите дату рождения: ");
                    continue;
                }

                employee.DateOfBirth = date;
                break;
            }
            else
            {
                Console.WriteLine("Неверный формат даты. Используйте дд.мм.гггг");
            }
        }
    }

    private void ReadSalary(Employee employee)
    {
        while (true)
        {
            Console.Write("Введите зарплату (или Enter чтобы пропустить): ");
            var input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input))
            {
                employee.Salary = null; // Пользователь пропустил ввод
                break;
            }

            if (!decimal.TryParse(input, out var salary))
            {
                Console.WriteLine("Неверный формат числа");
                continue;
            }

            if (salary < 0)
            {
                Console.WriteLine("Зарплата не может быть отрицательной");
                continue;
            }

            employee.Salary = salary;
            break;
        }
    }
}
