using Microsoft.EntityFrameworkCore;

namespace MediaStorage.Api.Infrastructure;

public class MediaStorageDbContext(DbContextOptions<MediaStorageDbContext> options) : DbContext(options)
{
    public DbSet<StoredFile> Files => Set<StoredFile>();
}