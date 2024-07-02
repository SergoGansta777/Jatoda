using AutoMapper;
using JatodaBackendApi.Models.DBModels;
using JatodaBackendApi.Models.Exceptions;
using JatodaBackendApi.Models.ModelViews;
using JatodaBackendApi.Providers.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JatodaBackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class ToDoController : ControllerBase
{
    private readonly IFileProvider _fileProvider;
    private readonly ILogger<ToDoController> _logger;
    private readonly IMapper _mapper;
    private readonly ITodoProvider<Todonote> _todoProvider;

    public ToDoController(ITodoProvider<Todonote> todoProvider, ILogger<ToDoController> logger,
        IMapper mapper, IFileProvider fileProvider)
    {
        _todoProvider = todoProvider;
        _logger = logger;
        _mapper = mapper;
        _fileProvider = fileProvider;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var todos = await _todoProvider.GetAllTodosAsync();
        if (todos == null)
        {
            return Ok(todos);
        }

        var mappedTodos = todos.Select(t => _mapper.Map<TodonoteViewModel>(t)).ToList();
        return Ok(mappedTodos);
    }

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

    [HttpGet("todos/{userId:int}")]
    public async Task<IActionResult> GetTodosByUserId(int userId)
    {
        var todos = await _todoProvider.GetTodosByUserIdAsync(userId);
        if (todos == null)
        {
            return Ok(todos);
        }

        todos = todos.Where(t => t.Completedon == null).ToList();
        var mappedTodos = todos.Select(t => _mapper.Map<TodonoteViewModel>(t)).ToList();
        _logger.LogInformation("Retrieved todos from the repository");
        return Ok(mappedTodos);
    }

    [HttpGet("completedtodos/{userId:int}")]
    public async Task<IActionResult> GetCompletedTodosByUserId(int userId)
    {
        var todos = await _todoProvider.GetTodosByUserIdAsync(userId);
        if (todos == null)
        {
            return Ok(todos);
        }

        todos = todos.Where(t => t.Completedon != null).ToList();
        var mappedTodos = todos.Select(t => _mapper.Map<TodonoteViewModel>(t)).ToList();
        _logger.LogInformation("Retrieved completed todos from the repository");
        return Ok(mappedTodos);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] TodonoteViewModel todo)
    {
        if (todo.file != null)
        {
            await _fileProvider.UploadFileAsync(todo.file);
        }

        var newTodo = new Todonote
        {
            Name = todo.Name,
            Notes = todo.Notes,
            Userid = todo.Userid,
            Difficultylevel = 1,
            Tags = new List<Tag>(),
            Multimediafilepath = todo.file?.FileName
        };

        var createdTodo = await _todoProvider.AddTodoAsync(newTodo);
        var mappedTodo = _mapper.Map<TodonoteViewModel>(createdTodo);
        return CreatedAtAction(nameof(GetById), new {id = createdTodo.Id}, mappedTodo);
    }

    [HttpGet("fileoftodo/{id:int}")]
    public async Task<IActionResult> GetFile(int id)
    {
        var todo = await _todoProvider.GetTodoByIdAsync(id);
        if (todo is null)
        {
            throw new TodoNotFoundException(id);
        }

        var fileName = todo.Multimediafilepath;
        var fileStream = await _fileProvider.GetFileAsync(fileName);

        if (fileStream == null)
        {
            throw new FileWithNameNotFoundException(fileName);
        }

        return File(fileStream, "application/octet-stream"); // return the file
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] TodonoteViewModel todo)
    {
        var existingTodo = await _todoProvider.GetTodoByIdAsync(id);
        if (existingTodo is null)
        {
            throw new TodoNotFoundException(id);
        }
        // TODO: convert here


        await _todoProvider.UpdateTodoAsync(existingTodo);
        return NoContent();
    }

    [HttpPut("complete/{id:int}")]
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

        existingTodo.Completedon = DateTime.Parse(requestModelView.CompletedOn).ToUniversalTime();
        await _todoProvider.UpdateTodoAsync(existingTodo);
        return NoContent();
    }


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