using AutoMapper;
using JatodaBackendApi.Models;
using JatodaBackendApi.ModelViews;
using JatodaBackendApi.Providers.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JatodaBackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class ToDoController : ControllerBase
{
    private readonly ILogger<AuthenticationController> _logger;
    private readonly ITodoProvider<Todonote> _todoProvider;
    private readonly IUserProvider<User> _userProvider;
    private readonly IMapper _mapper;

    public ToDoController(ITodoProvider<Todonote> todoProvider, ILogger<AuthenticationController> logger, IUserProvider<User> userProvider, IMapper mapper)
    {
        _todoProvider = todoProvider;
        _logger = logger;
        _userProvider = userProvider;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var todos = await _todoProvider.GetAllTodosAsync();
        if (todos == null) return NotFound();
        var mappedTodos = todos.Select(t => _mapper.Map<TodonoteViewModel>(t)).ToList();
        return Ok(mappedTodos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var todo = await _todoProvider.GetTodoByIdAsync(id);
        if (todo == null) return NotFound();
        var mappedTodo = _mapper.Map<TodonoteViewModel>(todo);
        return Ok(mappedTodo);
    }
    
    [HttpGet("todos/{userId}")]
    public async Task<IActionResult> GetTodosByUserId(int userId)
    {
        var todos = await _todoProvider.GetTodosByUserIdAsync(userId);
        todos = todos.Where(t => t.Completedon == null).ToList();
        if (todos == null) return Ok(todos);
        var mappedTodos = todos.Select(t => _mapper.Map<TodonoteViewModel>(t)).ToList();
        return Ok(mappedTodos);
    }
    [HttpGet("completedtodos/{userId}")]
    public async Task<IActionResult> GetCompletedTodosByUserId(int userId)
    {
        var todos = await _todoProvider.GetTodosByUserIdAsync(userId);
        todos = todos.Where(t => t.Completedon != null).ToList();
        if (todos == null) return Ok(todos);
        var mappedTodos = todos.Select(t => _mapper.Map<TodonoteViewModel>(t)).ToList();
        return Ok(mappedTodos);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] TodonoteViewModel todo)
    {
        var newTodo = new Todonote()
        {
            Name = todo.Name,   
            Notes = todo.Notes,
            Userid = todo.Userid,
            Difficultylevel = 1,
            Tags = new List<Tag>(),
        };
        
        var createdTodo = await _todoProvider.AddTodoAsync(newTodo);
        var mappedTodo = _mapper.Map<TodonoteViewModel>(createdTodo);
        return CreatedAtAction(nameof(GetById), new {id = createdTodo}, mappedTodo);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] TodonoteViewModel todo)
    {
        var existingTodo = await _todoProvider.GetTodoByIdAsync(id);
        if (existingTodo == null) return NotFound();
        // TODO: convert here


        await _todoProvider.UpdateTodoAsync(existingTodo);
        return NoContent();
    }
    
    [HttpPut("complete/{id}")]
    public async Task<IActionResult> Complete(int id, [FromBody]  CompleteRequest request)
    {
        var existingTodo = await _todoProvider.GetTodoByIdAsync(id);
        if (existingTodo == null) return NotFound();
        existingTodo.Completedon = DateTime.Parse(request.CompletedOn).ToUniversalTime();


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