using AutoMapper;
using Jatoda.Application.Core.Models.Dtos;
using Jatoda.Domain.Data.DBModels;

namespace Jatoda.Application.Core.Mappers;

public class TodoToTodoDto : Profile
{
    public TodoToTodoDto()
    {
        CreateMap<Todo, TodoDto>();
    }
}