namespace JatodaBackendApi.ModelViews;

public class TodonoteViewModel
{
    public int Id { get; set; }
    public string? Notes { get; set; }
    public int Userid { get; set; }

    public string Name { get; set; } = null!;
    public DateTime? CompletedOn { get; set; }
}