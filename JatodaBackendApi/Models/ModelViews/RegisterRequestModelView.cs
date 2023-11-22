using System.ComponentModel.DataAnnotations;

namespace JatodaBackendApi.Models.ModelViews;

public class RegisterRequestModelView
{
    [Required(ErrorMessage = "Username is required")]
    public string? Username { get; set; }
    [EmailAddress]
    [Required(ErrorMessage = "Email is required")]
    public string? Email { get; set; }
    [Required(ErrorMessage = "Password is required")]
    public string? Password { get; set; }
}