using AutoMapper;
using Jatoda.Application.Core.Models.RequestModels;
using Jatoda.Domain.Data.DBModels;

namespace Jatoda.Application.Core.Mappers;

public class CreateTodoRequestModelToTodo : Profile
{
    private CreateTodoRequestModelToTodo()
    {
        CreateMap<CreateTodoRequestModel, Todo>();
    }
}