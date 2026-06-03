public class StoredFile
{
    public Guid Id { get; init; }
    public Guid OwnerId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string BlobName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public string Extension { get; init; } = string.Empty;
    public long FileSize { get; init; }
    public string Category { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}