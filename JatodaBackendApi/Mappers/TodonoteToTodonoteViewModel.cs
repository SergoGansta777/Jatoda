using AutoMapper;
using JatodaBackendApi.Models.DBModels;
using JatodaBackendApi.Models.ModelViews;

namespace JatodaBackendApi.Mappers;

public class TodonoteToTodonoteViewModel : Profile
{
    public TodonoteToTodonoteViewModel()
    {
        CreateMap<Todonote, TodonoteViewModel>()
            .ForMember(d => d.Id, m => m.MapFrom(w => w.Id))
            .ForMember(d => d.Name, m => m.MapFrom(w => w.Name))
            .ForMember(d => d.Notes, m => m.MapFrom(w => w.Notes))
            .ForMember(d => d.Userid, m => m.MapFrom(w => w.Userid))
            .ForMember(d => d.CompletedOn, m => m.MapFrom(w => w.Completedon))
            .ForMember(d => d.file, m => m.MapFrom(w => w.Multimediafilepath));
    }
}