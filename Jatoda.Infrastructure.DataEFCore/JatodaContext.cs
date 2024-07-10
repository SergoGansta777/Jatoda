using Jatoda.Domain.Data.DBModels;
using Microsoft.EntityFrameworkCore;

namespace Jatoda.Infrastructure.DataEFCore;

public class JatodaContext : DbContext
{
    public JatodaContext()
    {
    }

    public JatodaContext(DbContextOptions options)
        : base(options)
    {
    }

    public virtual DbSet<FileMetadata> FileMetadata { get; set; }

    public virtual DbSet<Filetype> Filetypes { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<Todo> Todos { get; set; }

    public virtual DbSet<User> Users { get; set; }
}