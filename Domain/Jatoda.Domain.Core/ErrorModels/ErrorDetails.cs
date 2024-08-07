using System.Text.Json;

namespace Jatoda.Domain.Core.ErrorModels;

public class ErrorDetails
{
    public int StatusCode { get; set; }
    public string? Message { get; set; }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}