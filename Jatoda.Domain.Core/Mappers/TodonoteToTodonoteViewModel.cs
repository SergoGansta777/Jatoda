using AutoMapper;
using Jatoda.Application.Core.Models.ModelViews;
using Jatoda.Domain.Data.DBModels;

namespace Jatoda.Domain.Core.Mappers;

public class TodonoteToTodonoteViewModel : Profile
{
    public TodonoteToTodonoteViewModel()
    {
        CreateMap<Todo, TodonoteViewModel>();
    }
}