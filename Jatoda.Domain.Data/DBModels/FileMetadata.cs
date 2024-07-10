namespace Jatoda.Domain.Data.DBModels;

public class FileMetadata
{
    public int Id { get; set; }

    public int TodoId { get; set; }

    public string Filename { get; set; } = null!;

    public string Filetype { get; set; } = null!;

    public long Filesize { get; set; }

    public string Filepath { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public DateTime UpdateDate { get; set; }

    public virtual Todo Todo { get; set; } = null!;
}