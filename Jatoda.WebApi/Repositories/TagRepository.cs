using Jatoda.Factories;
using Jatoda.Models.DBModels;

namespace Jatoda.Repositories;

public class TagRepository(JatodaContext context) : RepositoryBase<Tag>(context), ITagRepository
{
    private readonly JatodaContext _context = context;
}