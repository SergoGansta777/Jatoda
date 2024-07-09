using JatodaBackendApi.Factories;
using JatodaBackendApi.Models.DBModels;

namespace JatodaBackendApi.Repositories;

public class UserRepository(JatodaContext context) : RepositoryBase<User>(context), IUserRepository
{
    private readonly JatodaContext _context = context;
}