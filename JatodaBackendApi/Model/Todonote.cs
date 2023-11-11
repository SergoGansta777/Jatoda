namespace JatodaBackendApi.Model;

public class Todonote
{
    public int Id { get; set; }

    public int Userid { get; set; }

    public string Name { get; set; } = null!;

    public int? Difficultylevel { get; set; }

    public string? Multimediafilepath { get; set; }

    public string? Notes { get; set; }

    public DateTime? Createdat { get; set; }

    public DateTime? Updatedat { get; set; }

    public virtual ICollection<Filemetadatum> Filemetadata { get; set; } = new List<Filemetadatum>();

    public virtual User User { get; set; } = null!;

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}