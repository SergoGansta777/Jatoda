using JatodaBackendApi.Model;
using JatodaBackendApi.Providers.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JatodaBackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize(AuthenticationSchemes = "Bearer")]
public class ToDoController : ControllerBase
{
    private readonly ILogger<AuthenticationController> _logger;
    private readonly ITodoProvider<Todonote> _todoProvider;

    public ToDoController(ITodoProvider<Todonote> todoProvider, ILogger<AuthenticationController> logger)
    {
        _todoProvider = todoProvider;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var todos = await _todoProvider.GetAllTodosAsync();
        return Ok(todos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var todo = await _todoProvider.GetTodoByIdAsync(id);
        if (todo == null) return NotFound();

        return Ok(todo);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] Todonote todo)
    {
        var createdTodo = await _todoProvider.AddTodoAsync(todo);
        return CreatedAtAction(nameof(GetById), new {id = createdTodo.Id}, createdTodo);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Todonote todo)
    {
        var existingTodo = await _todoProvider.GetTodoByIdAsync(todo.Id);
        if (existingTodo == null) return NotFound();
        existingTodo.Name = todo.Name;
        existingTodo.Notes = todo.Notes;
        existingTodo.Tags = todo.Tags;


        await _todoProvider.UpdateTodoAsync(existingTodo);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existingTodo = await _todoProvider.GetTodoByIdAsync(id);
        if (existingTodo == null) return NotFound();

        await _todoProvider.DeleteTodoAsync(existingTodo);
        return NoContent();
    }
}