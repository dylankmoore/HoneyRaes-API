using HoneyRaes_API.HoneyRaes_API.Models;

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
        Emergency = false
    },
    new ServiceTicket()
    {
        Id = 4,
        CustomerId = 1,
        EmployeeId = 1,
        Description = "Lock change",
        Emergency = false
    },
    new ServiceTicket()
    {
        Id = 5,
        CustomerId = 2,
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


//get service tickets
app.UseHttpsRedirection();
app.MapGet("/servicetickets", () =>
{
    return serviceTickets;
});

//get service tickets by id
app.MapGet("/servicetickets/{id}", (int id) =>
{
    ServiceTicket serviceTicket = serviceTickets.FirstOrDefault(st => st.Id == id);
    if (serviceTicket == null)
    {
        return Results.NotFound();
    }
    serviceTicket.Employee = employees.FirstOrDefault(e => e.Id == serviceTicket.EmployeeId);
    return Results.Ok(serviceTicket);
});

//get employees
app.MapGet("/employees", () =>
{
    return employees;
});

//get employees by id
app.MapGet("/employees/{id}", (int id) =>
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
app.MapGet("customers", () =>
{
    return customers;
});

//get customers by id
app.MapGet("customers/{id}", (int id) =>
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
app.MapPost("/servicetickets", (ServiceTicket serviceTicket) =>
{
    // creates a new id (When we get to it later, our SQL database will do this for us like JSON Server did!)
    serviceTicket.Id = serviceTickets.Max(st => st.Id) + 1;
    serviceTickets.Add(serviceTicket);
    return serviceTicket;
});

//deleting service tickets
app.MapDelete("/servicetickets/{id}", (int id) =>
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
app.MapPut("/servicetickets/{id}", (int id, ServiceTicket serviceTicket) =>
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
app.MapPost("/servicetickets/{id}/complete", (int id) =>
{
    ServiceTicket ticketToComplete = serviceTickets.FirstOrDefault(st => st.Id == id);
    ticketToComplete.DateCompleted = DateTime.Today;
});

app.Run();
