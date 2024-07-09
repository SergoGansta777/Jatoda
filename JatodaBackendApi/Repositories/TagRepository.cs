using JatodaBackendApi.Factories;
using JatodaBackendApi.Models.DBModels;

namespace JatodaBackendApi.Repositories;

public class TagRepository(JatodaContext context) : RepositoryBase<Tag>(context), ITagRepository
{
    private readonly JatodaContext _context = context;
}