using Jatoda.Application.Interfaces;
using Jatoda.Domain.Core.DBModels;

namespace Jatoda.Infrastructure.EFCore.Repositories;

public class TagRepository(JatodaContext context) : RepositoryBase<Tag>(context), ITagRepository
{
}