using MediaStorage.Api.Contracts;
using MediaStorage.Api.Infrastructure;
using MediaStorage.Api.Services;
using MediaStorage.Api.Domain;
using Microsoft.AspNetCore.Mvc;

namespace MediaStorage.Api.Controllers;

[ApiController]
[Route("api/files")]
public class FilesController(
    MediaStorageDbContext dbContext,
    BlobStorageService blobStorageService)
    : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> UploadFile(IFormFile file, [FromForm] string category, CancellationToken cancellationToken)
    {
        if (file.Length == 0)
        {
            return BadRequest("File is required.");
        }

        if (string.IsNullOrWhiteSpace(category))
        {
            return BadRequest("Category is required.");
        }

        var fileId = Guid.NewGuid();

        var extension = Path.GetExtension(file.FileName);

        var blobName = $"{category}/{fileId}{extension}";

        await using var stream = file.OpenReadStream();

        await blobStorageService.UploadAsync(stream, blobName, file.ContentType, cancellationToken);

        var storedFile = new StoredFile
        {
            Id = fileId,
            OwnerId = Guid.Empty, // change with JWT user id
            FileName = file.FileName,
            BlobName = blobName,
            ContentType = file.ContentType,
            Extension = extension,
            Size = file.Length,
            Category = category,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Files.Add(storedFile);

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new UploadFileResponse
        {
            FileId = storedFile.Id,
            FileName = storedFile.FileName,
            ContentType = storedFile.ContentType,
            Size = storedFile.Size,
            Category = storedFile.Category
        };

        return Created($"/api/files/{storedFile.Id}", response);
    }
}