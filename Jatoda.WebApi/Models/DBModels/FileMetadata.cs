namespace Jatoda.Models.DBModels;

public class FileMetadata
{
    public int Id { get; set; }

    public int Todonoteid { get; set; }

    public string Filename { get; set; } = null!;

    public string Filetype { get; set; } = null!;

    public long Filesize { get; set; }

    public string Filepath { get; set; } = null!;

    public DateTime Createdat { get; set; }

    public DateTime Updatedat { get; set; }

    public virtual Todo Todo { get; set; } = null!;
}