using Jatoda.Application.Interfaces;
using Jatoda.Domain.Core.DBModels;

namespace Jatoda.Infrastructure.DataEFCore.Repositories;

public class TagRepository(JatodaContext context) : RepositoryBase<Tag>(context), ITagRepository
{
    private readonly JatodaContext _context = context;
}