using System.ComponentModel.DataAnnotations;

namespace Jatoda.Models.DBModels;

public class User
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Username is a required field.")]
    [MaxLength(60, ErrorMessage = "Maximum length for the Username is 60 characters.")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Email is required field")]
    public string Email { get; set; }

    [Required(ErrorMessage = "PasswordHash is required field")]
    public string? PasswordHash { get; set; }

    public DateTime CreateDate { get; set; }

    public DateTime UpdateDate { get; set; }

    public virtual ICollection<Todo> Todos { get; set; }

    public virtual ICollection<Role> Roles { get; set; }
}