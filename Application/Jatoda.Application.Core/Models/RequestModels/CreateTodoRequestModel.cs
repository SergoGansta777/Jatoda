namespace Jatoda.Application.Core.Models.RequestModels;

public class CreateTodoRequestModel
{
    public Guid Userid { get; set; }
    public string? Notes { get; set; }
    public string Name { get; set; } = null!;
}