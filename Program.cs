using Microsoft.EntityFrameworkCore;
using MinimalApiExploration.Models;

var builder = WebApplication.CreateBuilder(args);

// Entity Framework Configuration
builder.Services.AddDbContext<TodoDb>(option => option.UseInMemoryDatabase("TodoListDb"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();

app.MapGet("/", () => "Welcome to your first Minimal API!");

app.MapGet("hello/{name}", (string name) => $"Hello, {name}");

app.MapGet("/person", () => new {Name = "Qadeer", Age = 30 });

// Todo Minimal Apis.

// Seeding Initial Data.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TodoDb>();
    if (!db.Todos.Any())
    {
        db.Todos.AddRange(new[]
        {
            new Todo { Name = "Learn .NET 8", IsComplete = false },
            new Todo { Name = "Write Documentation", IsComplete = true }
        });
        db.SaveChanges();
    }
}

// CREATE new.
app.MapPost("/todoitems/", async (Todo todo, TodoDb db) =>
{
    await db.AddAsync(todo);
    await db.SaveChangesAsync();
    return Results.Created($"/todoitems/{todo.Id}", todo);
});

// GET all.
app.MapGet("/todoitems", async (TodoDb db) => await db.Todos.ToListAsync());

// GET based on condition.
app.MapGet("/todoitems/complete", async (TodoDb db) => await db.Todos.Where(c => c.IsComplete).ToListAsync());

// GET/SEARCH by Id.
app.MapGet("/todoitems/{id}", async (int id, TodoDb db) =>
{
    var todo = await db.Todos.FindAsync(id);
    if (todo != null)
    {
        return Results.Ok(todo);
    }
    else
    {
        return Results.NotFound();
    }
});

// UPDATING by Id.

app.MapPut("/todoitems/{id}", async (int id, Todo updatedTodo, TodoDb db) =>
{
    var todo = await db.Todos.FindAsync(id);
    if (todo is null) return Results.NotFound();
    todo.Name = updatedTodo.Name;
    todo.IsComplete = updatedTodo.IsComplete;

    await db.SaveChangesAsync();
    return Results.NoContent();

});

// DELETE by Id.
app.MapDelete("/todoitems/{id}", async (int id, TodoDb db) =>
{
    if(await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    return Results.NotFound();
});

app.Run();
