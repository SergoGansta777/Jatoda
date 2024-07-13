using AutoMapper;
using Jatoda.Application.Core.Models.ModelViews;
using Jatoda.Domain.Core.DBModels;

namespace Jatoda.Application.Core.Mappers;

public class TodoToTodoDto : Profile
{
    public TodoToTodoDto()
    {
        CreateMap<Todo, TodoDto>();
    }
}