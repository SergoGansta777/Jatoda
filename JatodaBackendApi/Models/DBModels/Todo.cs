using System.ComponentModel.DataAnnotations;

namespace JatodaBackendApi.Models.DBModels;

public class Todo
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "UserId is required field")]
    public Guid UserId { get; set; }

    [Required(ErrorMessage = "Name of todo is required field")]
    public string Name { get; set; } = null!;

    public int? DifficultyLevel { get; set; }

    public string? MultimediaFilePath { get; set; }

    public string? Notes { get; set; }

    public DateTime CreateDate { get; set; }

    public DateTime UpdateDate { get; set; }

    public DateTime? CompletedOn { get; set; }

    public virtual ICollection<FileMetadata> FileMetadata { get; set; }

    public virtual User User { get; set; }

    public virtual ICollection<Tag> Tags { get; set; }
}