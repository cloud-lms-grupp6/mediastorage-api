namespace MediaStorage.Api.Contracts;

public class FileResponse
{
    public Guid FileId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long Size { get; init; }
    public string Category { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}