namespace dotnet.Models;

public sealed class ErrorResponse
{
    public required IList<string> Errors { get; set; }
}
