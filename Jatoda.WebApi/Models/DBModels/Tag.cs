using System.ComponentModel.DataAnnotations;

namespace Jatoda.Models.DBModels;

public class Tag
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Tag name is required field")]
    [MaxLength(10, ErrorMessage = "Maximum length for the Name of tag is 10 characters.")]
    public string Name { get; set; }

    public virtual ICollection<Todo> Todos { get; set; }
}