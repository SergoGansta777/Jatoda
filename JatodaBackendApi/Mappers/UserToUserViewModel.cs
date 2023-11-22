using AutoMapper;
using JatodaBackendApi.Models.DBModels;
using JatodaBackendApi.Models.ModelViews;

namespace JatodaBackendApi.Mappers;

public class UserToUserViewModel : Profile
{
    public UserToUserViewModel()
    {
        CreateMap<User, UserViewModel>();
    }
}