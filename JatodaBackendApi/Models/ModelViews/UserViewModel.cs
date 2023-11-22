using System.ComponentModel.DataAnnotations;

namespace JatodaBackendApi.Models.ModelViews;

public class UserViewModel
{
    [Required]
    public int Id { get; set; }

    [Required]
    public string? Username { get; set; } = null!;

}