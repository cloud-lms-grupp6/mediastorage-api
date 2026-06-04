using MediaStorage.Api.Contracts;
using MediaStorage.Api.Infrastructure;
using MediaStorage.Api.Services;
using MediaStorage.Api.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace MediaStorage.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/files")]
public class FilesController(MediaStorageDbContext dbContext, BlobStorageService blobStorageService) : ControllerBase
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

        var role = User.FindFirst("role")?.Value;
        if (category == "lectures" && role != "Teacher")
        {
            return Forbid();
        }

        var fileId = Guid.NewGuid();

        var extension = Path.GetExtension(file.FileName);

        var blobName = $"{category}/{fileId}{extension}";

        await using var stream = file.OpenReadStream();

        await blobStorageService.UploadAsync(stream, blobName, file.ContentType, cancellationToken);

        var userId = Guid.Parse(User.FindFirst("uid")!.Value);

        var storedFile = new StoredFile
        {
            Id = fileId,
            OwnerId = userId,
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



    [HttpGet("{fileId:guid}")]
    public async Task<IActionResult> GetFile(Guid fileId,CancellationToken cancellationToken)
    {
        var file = await dbContext.Files.FindAsync([fileId], cancellationToken);

        if (file is null)
        {
            return NotFound();
        }

        var userId = Guid.Parse(User.FindFirst("uid")!.Value);
        if (file.OwnerId != userId)
        {
            return Forbid();
        }

        var response = new FileResponse
        {
            FileId = file.Id,
            FileName = file.FileName,
            ContentType = file.ContentType,
            Size = file.Size,
            Category = file.Category,
            CreatedAt = file.CreatedAt
        };

        return Ok(response);
    }


    [HttpDelete("{fileId:guid}")]
    public async Task<IActionResult> DeleteFile(Guid fileId,CancellationToken cancellationToken)
    {
        var file = await dbContext.Files.FindAsync([fileId],cancellationToken);

        if (file is null)
        {
            return NotFound();
        }

        var userId = Guid.Parse(User.FindFirst("uid")!.Value);

        if (file.OwnerId != userId)
        {
            return Forbid();
        }

        await blobStorageService.DeleteAsync(file.BlobName,cancellationToken);

        dbContext.Files.Remove(file);

        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}