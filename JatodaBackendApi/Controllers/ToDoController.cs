using AutoMapper;
using JatodaBackendApi.Models.DBModels;
using JatodaBackendApi.Models.Exceptions;
using JatodaBackendApi.Models.ModelViews;
using JatodaBackendApi.Providers.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JatodaBackendApi.Controllers;

/// <summary>
///     Controller for managing ToDo operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class ToDoController : ControllerBase
{
    private readonly IFileProvider _fileProvider;
    private readonly ILogger<ToDoController> _logger;
    private readonly IMapper _mapper;
    private readonly ITodoProvider<Todo> _todoProvider;

    public ToDoController(ITodoProvider<Todo> todoProvider, ILogger<ToDoController> logger,
        IMapper mapper, IFileProvider fileProvider)
    {
        _todoProvider = todoProvider;
        _logger = logger;
        _mapper = mapper;
        _fileProvider = fileProvider;
    }

    /// <summary>
    ///     Get all ToDo items.
    /// </summary>
    /// <returns>List of ToDo items.</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var todos = await _todoProvider.GetAllTodosAsync();
        if (todos is null)
        {
            return Ok(todos);
        }

        var mappedTodos = todos.Select(t => _mapper.Map<TodonoteViewModel>(t)).ToList();
        return Ok(mappedTodos);
    }

    /// <summary>
    ///     Get a specific ToDo item by its ID.
    /// </summary>
    /// <param name="id">ID of the ToDo item.</param>
    /// <returns>ToDo item with the specified ID.</returns>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var todo = await _todoProvider.GetTodoByIdAsync(id);
        if (todo is null)
        {
            throw new TodoNotFoundException(id);
        }

        var mappedTodo = _mapper.Map<TodonoteViewModel>(todo);
        return Ok(mappedTodo);
    }

    /// <summary>
    ///     Get all ToDo items for a specific user.
    /// </summary>
    /// <param name="userId">ID of the user.</param>
    /// <returns>List of ToDo items for the user.</returns>
    [HttpGet("users/{userId:int}/todos")]
    public async Task<IActionResult> GetTodosByUserId(int userId)
    {
        var todos = await _todoProvider.GetTodosByUserIdAsync(userId);
        if (todos is null)
        {
            return Ok(todos);
        }

        todos = todos.Where(t => t.CompletedOn is null).ToList();
        var mappedTodos = todos.Select(t => _mapper.Map<TodonoteViewModel>(t)).ToList();
        _logger.LogInformation("Retrieved todos from the repository");
        return Ok(mappedTodos);
    }

    /// <summary>
    ///     Get all completed ToDo items for a specific user.
    /// </summary>
    /// <param name="userId">ID of the user.</param>
    /// <returns>List of completed ToDo items for the user.</returns>
    [HttpGet("users/{userId:int}/completed-todos")]
    public async Task<IActionResult> GetCompletedTodosByUserId(int userId)
    {
        var todos = await _todoProvider.GetTodosByUserIdAsync(userId);
        if (todos is null)
        {
            return Ok(todos);
        }

        todos = todos.Where(t => t.CompletedOn is not null).ToList();
        var mappedTodos = todos.Select(t => _mapper.Map<TodonoteViewModel>(t)).ToList();
        _logger.LogInformation("Retrieved completed todos from the repository");
        return Ok(mappedTodos);
    }

    /// <summary>
    ///     Add a new ToDo item.
    /// </summary>
    /// <param name="todo">ToDo item data.</param>
    /// <returns>Created ToDo item.</returns>
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] TodonoteViewModel todo)
    {
        if (todo.File is not null)
        {
            await _fileProvider.UploadFileAsync(todo.File);
        }

        var newTodo = _mapper.Map<Todo>(todo);
        var createdTodo = await _todoProvider.AddTodoAsync(newTodo);
        var mappedTodo = _mapper.Map<TodonoteViewModel>(createdTodo);
        return CreatedAtAction(nameof(GetById), new {id = createdTodo.Id}, mappedTodo);
    }

    /// <summary>
    ///     Download the file attached to a ToDo item.
    /// </summary>
    /// <param name="id">ID of the ToDo item.</param>
    /// <returns>File attached to the ToDo item.</returns>
    [HttpGet("{id:int}/file")]
    public async Task<IActionResult> GetFile(int id)
    {
        var todo = await _todoProvider.GetTodoByIdAsync(id);
        if (todo is null)
        {
            throw new TodoNotFoundException(id);
        }

        var fileName = todo.MultimediaFilePath;
        var fileStream = await _fileProvider.GetFileAsync(fileName!);

        if (fileStream is null)
        {
            throw new FileWithNameNotFoundException(fileName!);
        }

        return File(fileStream, "application/octet-stream");
    }

    /// <summary>
    ///     Update an existing ToDo item.
    /// </summary>
    /// <param name="id">ID of the ToDo item to update.</param>
    /// <param name="todo">Updated ToDo item data.</param>
    /// <returns>No content if successful.</returns>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] TodonoteViewModel todo)
    {
        var existingTodo = await _todoProvider.GetTodoByIdAsync(id);
        if (existingTodo is null)
        {
            throw new TodoNotFoundException(id);
        }

        _mapper.Map(todo, existingTodo);

        await _todoProvider.UpdateTodoAsync(existingTodo);
        return NoContent();
    }

    /// <summary>
    ///     Mark a ToDo item as completed.
    /// </summary>
    /// <param name="id">ID of the ToDo item to complete.</param>
    /// <param name="requestModelView">Request data containing the completion date.</param>
    /// <returns>No content if successful.</returns>
    [HttpPut("{id:int}/complete")]
    public async Task<IActionResult> Complete(int id, [FromBody] CompleteRequestModelView requestModelView)
    {
        var existingTodo = await _todoProvider.GetTodoByIdAsync(id);

        if (existingTodo is null)
        {
            throw new TodoNotFoundException(id);
        }

        if (requestModelView.CompletedOn is null)
        {
            throw new CompleteBadRequestException();
        }

        existingTodo.CompletedOn = DateTime.Parse(requestModelView.CompletedOn).ToUniversalTime();
        await _todoProvider.UpdateTodoAsync(existingTodo);
        return NoContent();
    }

    /// <summary>
    ///     Delete a ToDo item.
    /// </summary>
    /// <param name="id">ID of the ToDo item to delete.</param>
    /// <returns>No content if successful.</returns>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existingTodo = await _todoProvider.GetTodoByIdAsync(id);
        if (existingTodo is null)
        {
            throw new TodoNotFoundException(id);
        }

        await _todoProvider.DeleteTodoAsync(existingTodo);
        return NoContent();
    }
}