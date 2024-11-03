using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run(async(HttpContext context) =>
{
    //context.Request

    foreach (var key in context.Request.Query.Keys)
    {
        await context.Response.WriteAsync($"{key}: {context.Request.Query[key]}\r\n");
    }

   if(context.Request.Method == "GET")
    {
        if(context.Request.Path.StartsWithSegments("/"))
        {
            await context.Response.WriteAsync($"The method is:{context.Request.Method}\r\n");
            await context.Response.WriteAsync($"The URL is:{context.Request.Path}\r\n");

            await context.Response.WriteAsync($"\r\nHeaders:\r\n");
            foreach (var key in context.Request.Headers.Keys)
            {
                var res = context.Request.Headers[key];
                await context.Response.WriteAsync($"{key}: {context.Request.Headers[key]}\r\n");
            }
        }
        else if(context.Request.Path.StartsWithSegments("/employess"))
        {
            var employess = EmployeeRepository.GetEmployees();
            foreach (var item in employess)
            {
                await context.Response.WriteAsync($"{item.Name}: {item.Position}\r\n");
            }
        }
    }
   else if(context.Request.Method == "POST")
    {
        if(context.Request.Path.StartsWithSegments("/employess"))
        {
            using var reader = new StreamReader(context.Request.Body);
            var body = await reader.ReadToEndAsync();
            await context.Response.WriteAsync(body);
            var employee = JsonSerializer.Deserialize<Employee?>(body);
            EmployeeRepository.AddEmployee(employee);
        }
    }
   else if (context.Request.Method == "PUT")
    {
        if (context.Request.Path.StartsWithSegments("/employess"))
        {
            using var reader = new StreamReader(context.Request.Body);
            var body = await reader.ReadToEndAsync();
            
            var employee = JsonSerializer.Deserialize<Employee?>(body);
            var status =  EmployeeRepository.UpdateEmployee(employee);
            if(status)
            {
                await context.Response.WriteAsync("Employee Updated Successfully");
            }
            else 
            {
                await context.Response.WriteAsync("Employee Not Found");
            }
        }
    }
}); 
app.Run();

static class EmployeeRepository
{
    private static List<Employee> employess = new List<Employee>
    {
        new Employee(1,"rahul","Enginner",60000),
        new Employee(2,"rahul2","Developer",70000),
        new Employee(3,"rahul3","Civil",80000),

    };
    public static List<Employee> GetEmployees() => employess;
    public static void AddEmployee(Employee? employee)
    {
        if (employee is not null)
        {
            employess.Add(employee);
        }
    }
    public static bool UpdateEmployee(Employee? employee)
    {
        if(employee is not null)
        {
            var emp = employess.FirstOrDefault(x => x.Id == employee.Id);
            if (emp is not null)
            {
                emp.Name = employee.Name;
                emp.Position = employee.Position;
                emp.Salary = employee.Salary;
                return true;
            }
            
        }
        return false;
    }
}

public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Position { get; set; }
    public double Salary { get; set; }

   
    public Employee(int id, string name, string position, double salary)
    {
        Id = id;
        Name = name;
        Position = position;
        Salary = salary;
    }
}
