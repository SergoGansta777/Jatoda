using AutoMapper;
using JatodaBackendApi.Models.DBModels;
using JatodaBackendApi.Models.ModelViews;

namespace JatodaBackendApi.Mappers;

public class TodonoteToTodonoteViewModel : Profile
{
    public TodonoteToTodonoteViewModel()
    {
        CreateMap<Todo, TodonoteViewModel>()
            .ForMember(d => d.File, m => m.MapFrom(w => w.MultimediaFilePath));
    }
}