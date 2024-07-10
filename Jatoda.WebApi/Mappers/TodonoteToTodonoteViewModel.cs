using AutoMapper;
using Jatoda.Models.DBModels;
using Jatoda.Models.ModelViews;

namespace Jatoda.Mappers;

public class TodonoteToTodonoteViewModel : Profile
{
    public TodonoteToTodonoteViewModel()
    {
        CreateMap<Todo, TodonoteViewModel>()
            .ForMember(d => d.File, m => m.MapFrom(w => w.MultimediaFilePath));
    }
}