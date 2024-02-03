using HoneyRaes_API.HoneyRaes_API.Models;
using Microsoft.AspNetCore.Http.HttpResults;

List<Customer> customers = new List<Customer>
{
    new Customer()
    {
      Id = 1,
      Name = "Casey Becker",
      Address = "7420 Sonoma Mountain Road",
    },
    new Customer()
    {
        Id = 2,
        Name = "Sidney Prescott",
        Address = "1820 Calistoga Road",
    },
    new Customer()
    {
        Id = 3,
        Name = "Cotton Weary",
        Address = "1334 N Harper Ave",
    }
};

List<Employee> employees = new List<Employee>
{
    new Employee()
    {
        Id = 1,
        Name = "Gale Weathers",
        Specialty = "Journalism",
    },
    new Employee()
    {
        Id = 2,
        Name = "Roman Bridger",
        Specialty = "Film Direction"
    }
};

List<ServiceTicket> serviceTickets = new List<ServiceTicket>
{
    new ServiceTicket()
    {
        Id = 1,
        CustomerId = 1,
        EmployeeId = null,
        Description = "Burned popcorn",
        Emergency = true,
        DateCompleted = new DateTime (1996, 09, 25)
    },
    new ServiceTicket()
    {
        Id = 2,
        CustomerId = 2,
        EmployeeId = 1,
        Description = "Broken vase",
        Emergency = true,
        DateCompleted = new DateTime (1996, 09, 26)
    },
    new ServiceTicket()
    {
        Id = 3,
        CustomerId = 3,
        EmployeeId = 2,
        Description = "Phone repair",
        Emergency = false,
        DateCompleted = null
    },
    new ServiceTicket()
    {
        Id = 4,
        CustomerId = 1,
        EmployeeId = 1,
        Description = "Lock change",
        Emergency = false,
        DateCompleted = null
    },
    new ServiceTicket()
    {
        Id = 5,
        CustomerId = 2,
        EmployeeId = null,
        Description = "Broken garage door",
        Emergency = false,
        DateCompleted = new DateTime(1994, 09, 28)
    }
};

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();

//get service tickets
app.MapGet("/api/servicetickets", () =>
{
    return serviceTickets;
});

//get service tickets by id
app.MapGet("/api/servicetickets/{id}", (int id) =>
{
    ServiceTicket serviceTicket = serviceTickets.FirstOrDefault(st => st.Id == id);
    if (serviceTicket == null)
    {
        return Results.NotFound();
    }
    serviceTicket.Employee = employees.FirstOrDefault(e => e.Id == serviceTicket.EmployeeId);
    serviceTicket.Customer = customers.FirstOrDefault(c => c.Id == serviceTicket.CustomerId);
    return Results.Ok(serviceTicket);
});

//get employees
app.MapGet("/api/employees", () =>
{
    return employees;
});

//get employees by id
app.MapGet("/api/employees/{id}", (int id) =>
{
    Employee employee = employees.FirstOrDefault(e => e.Id == id);
    if (employee == null)
    {
        return Results.NotFound();
    }
    employee.ServiceTickets = serviceTickets.Where(st => st.EmployeeId == id).ToList();
    return Results.Ok(employee);
});

//get customers
app.MapGet("/api/customers", () =>
{
    return customers;
});

//get customers by id
app.MapGet("/api/customers/{id}", (int id) =>
{
    Customer customer = customers.FirstOrDefault(c => c.Id == id);
    if (customer == null)
    {
        return Results.NotFound();
    }
    customer.ServiceTickets = serviceTickets.Where(st => st.CustomerId == id).ToList();
    return Results.Ok(customer);
});

//creating new service tickets
app.MapPost("/api/servicetickets", (ServiceTicket serviceTicket) =>
{
    // creates a new id (When we get to it later, our SQL database will do this for us like JSON Server did!)
    serviceTicket.Id = serviceTickets.Max(st => st.Id) + 1;
    serviceTickets.Add(serviceTicket);
    return serviceTicket;
});

//deleting service tickets
app.MapDelete("/api/servicetickets/{id}", (int id) =>
{
    ServiceTicket serviceTicketToDelete = serviceTickets.FirstOrDefault(st => st.Id == id);

    if (serviceTicketToDelete == null)
    {
        return Results.NotFound();
    }

    serviceTickets.Remove(serviceTicketToDelete);
    return Results.Ok(serviceTicketToDelete);
});

//updating service ticket
app.MapPut("/api/servicetickets/{id}", (int id, ServiceTicket serviceTicket) =>
{

    ServiceTicket ticketToUpdate = serviceTickets.FirstOrDefault(st => st.Id == id);
    int ticketIndex = serviceTickets.IndexOf(ticketToUpdate);

    if (ticketToUpdate == null)
    {
        return Results.NotFound();
    }

    //the id in the request route doesn't match the id from the ticket in the request body. That's a bad request!

    if (id != serviceTicket.Id)
    {
        return Results.BadRequest();
    }
    serviceTickets[ticketIndex] = serviceTicket;
    return Results.Ok();
});

//updating completed tickets
app.MapPost("/api/servicetickets/{id}/complete", (int id) =>
{
    ServiceTicket ticketToComplete = serviceTickets.FirstOrDefault(st => st.Id == id);
    ticketToComplete.DateCompleted = DateTime.Today;
});

//get emergency tickets
app.MapGet("/api/servicetickets/emergency", () =>
{
    List<ServiceTicket> emergencyTickets = serviceTickets
    .Where(st => st.Emergency == true && st.DateCompleted == new DateTime()).ToList();
    if (emergencyTickets == null)
    {
        return Results.BadRequest();
    }
    return Results.Ok(emergencyTickets);
});

//get unassigned tickets
app.MapGet("/api/servicetickets/unassigned", () =>
{
    List<ServiceTicket> unassignedTickets = serviceTickets.Where(st => st.EmployeeId == null).ToList();
    return Results.Ok(unassignedTickets);
});


//get inactive customers
app.MapGet("/api/customers/inactive", () =>
{   //getting current year
    DateTime currentYear = DateTime.Now;

    //filtering tickets that have been completed over a year ago
    List<ServiceTicket> closedTickets = serviceTickets.Where(st => st.DateCompleted != null && st.DateCompleted < currentYear.AddYears(-1)).ToList();

    //filtering customers who do not have CustomerIds in closedTickets
    List<Customer> inactiveCustomers = customers.Where(c => closedTickets.Any(st => st.CustomerId == c.Id)).ToList();

    //returning list of inactive customers
    return Results.Ok(inactiveCustomers);
});


//get available employees
app.MapGet("/api/employees/available", () =>
{
    List<ServiceTicket> incompleteTicket = serviceTickets
    .Where(st => st.DateCompleted == null).ToList();
    List<Employee> unassignedEmployees = employees
    .Where(e => incompleteTicket
    .Any(st => st.EmployeeId != e.Id)).ToList();
    return Results.Ok(unassignedEmployees);
});

//get employee's customers
app.MapGet("/api/employees/{id}/customers", (int id) =>
{
    Employee employee = employees.FirstOrDefault(e => e.Id == id);
    List<ServiceTicket> assignedTickets = serviceTickets
    .Where(st => st.EmployeeId
    .Equals(id)).ToList();
    List<Customer> employeeCustomers = customers
    .Where(c => assignedTickets
    .Any(st => st.CustomerId == c.Id))
    .ToList();
    return Results.Ok(employeeCustomers);
});

//get employee of the month
//Create and endpoint to return the employee who has completed the most service tickets last month.
app.MapGet("/api/employees/eotm", () =>
{
    var lastMonth = DateTime.Now.AddMonths(-1);
    var employeeOfTheMonth = employees
        .OrderByDescending(e =>
            serviceTickets.Count(st =>
                st.EmployeeId == e.Id && st.DateCompleted.HasValue && st.DateCompleted.Value.Month == lastMonth.Month))
        .FirstOrDefault();

    return employeeOfTheMonth != null
        ? Results.Ok(employeeOfTheMonth)
        : Results.NotFound();
});

//get past ticket review
app.MapGet("/api/servicetickets/completed", () =>
{
    var completedTickets = serviceTickets
    .Where(st => st.DateCompleted != null)
    .OrderBy(st => st.DateCompleted);
    return Results.Ok(completedTickets);
});

//get prioritized tickets
app.MapGet("/api/servicetickets/prioritized", () =>
{
    var prioritizedTickets = serviceTickets
        .Where(st => st.DateCompleted == null)
        .OrderByDescending(st => st.Emergency)
        .ThenBy(st => st.EmployeeId.HasValue).ToList();

    return Results.Ok(prioritizedTickets);
});

app.Run();
