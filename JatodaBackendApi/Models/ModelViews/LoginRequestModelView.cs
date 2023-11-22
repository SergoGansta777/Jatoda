using System.ComponentModel.DataAnnotations;

namespace JatodaBackendApi.Models.ModelViews;

public class LoginRequestModelView
{
    [Required(ErrorMessage = "Username is required")]
    public string? Username { get; set; }
    [Required(ErrorMessage = "Password is required")]
    public string? Password { get; set; }
}