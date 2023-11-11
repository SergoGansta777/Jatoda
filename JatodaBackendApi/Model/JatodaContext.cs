using Microsoft.EntityFrameworkCore;

namespace JatodaBackendApi.Model;

public partial class JatodaContext : DbContext
{
    public JatodaContext()
    {
    }

    public JatodaContext(DbContextOptions<JatodaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Filemetadatum> Filemetadata { get; set; }

    public virtual DbSet<Filetype> Filetypes { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<Todonote> Todonotes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https: //go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Username=sergejnehorosev;Password=895318;DataBase=Jatoda");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Filemetadatum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("filemetadata_pkey");

            entity.ToTable("filemetadata");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("createdat");
            entity.Property(e => e.Filename)
                .HasMaxLength(255)
                .HasColumnName("filename");
            entity.Property(e => e.Filepath)
                .HasMaxLength(255)
                .HasColumnName("filepath");
            entity.Property(e => e.Filesize).HasColumnName("filesize");
            entity.Property(e => e.Filetype)
                .HasMaxLength(50)
                .HasColumnName("filetype");
            entity.Property(e => e.Todonoteid).HasColumnName("todonoteid");
            entity.Property(e => e.Updatedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updatedat");

            entity.HasOne(d => d.Todonote).WithMany(p => p.Filemetadata)
                .HasForeignKey(d => d.Todonoteid)
                .HasConstraintName("filemetadata_todonoteid_fkey");
        });

        modelBuilder.Entity<Filetype>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("filetypes_pkey");

            entity.ToTable("filetypes");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("roles_pkey");

            entity.ToTable("roles");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tags_pkey");

            entity.ToTable("tags");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Todonote>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("todonotes_pkey");

            entity.ToTable("todonotes");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("createdat");
            entity.Property(e => e.Difficultylevel).HasColumnName("difficultylevel");
            entity.Property(e => e.Multimediafilepath)
                .HasMaxLength(255)
                .HasColumnName("multimediafilepath");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.Updatedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updatedat");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.User).WithMany(p => p.Todonotes)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("todonotes_userid_fkey");

            entity.HasMany(d => d.Tags).WithMany(p => p.Todonotes)
                .UsingEntity<Dictionary<string, object>>(
                    "Todonotetag",
                    r => r.HasOne<Tag>().WithMany()
                        .HasForeignKey("Tagid")
                        .HasConstraintName("todonotetags_tagid_fkey"),
                    l => l.HasOne<Todonote>().WithMany()
                        .HasForeignKey("Todonoteid")
                        .HasConstraintName("todonotetags_todonoteid_fkey"),
                    j =>
                    {
                        j.HasKey("Todonoteid", "Tagid").HasName("todonotetags_pkey");
                        j.ToTable("todonotetags");
                        j.IndexerProperty<int>("Todonoteid").HasColumnName("todonoteid");
                        j.IndexerProperty<int>("Tagid").HasColumnName("tagid");
                    });
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "uq_users_email").IsUnique();

            entity.HasIndex(e => e.Username, "uq_users_username").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("createdat");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Passwordhash)
                .HasMaxLength(255)
                .HasColumnName("passwordhash");
            entity.Property(e => e.Passwordsalt)
                .HasMaxLength(255)
                .HasColumnName("passwordsalt");
            entity.Property(e => e.Updatedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updatedat");
            entity.Property(e => e.Username)
                .HasMaxLength(255)
                .HasColumnName("username");

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "Userrole",
                    r => r.HasOne<Role>().WithMany()
                        .HasForeignKey("Roleid")
                        .HasConstraintName("userroles_roleid_fkey"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("Userid")
                        .HasConstraintName("userroles_userid_fkey"),
                    j =>
                    {
                        j.HasKey("Userid", "Roleid").HasName("userroles_pkey");
                        j.ToTable("userroles");
                        j.IndexerProperty<int>("Userid").HasColumnName("userid");
                        j.IndexerProperty<int>("Roleid").HasColumnName("roleid");
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}