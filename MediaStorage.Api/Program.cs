using MediaStorage.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using MediaStorage.Api.Contracts;
using MediaStorage.Api.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddOpenApi();
builder.Services.Configure<BlobStorageOptions>(builder.Configuration.GetSection(BlobStorageOptions.SectionName));
builder.Services.AddScoped<BlobStorageService>();

var connectionString = builder.Configuration.GetConnectionString("MediaStorageDatabase");

builder.Services.AddDbContext<MediaStorageDbContext>(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.UseSqlite(connectionString);     
    }
    else
    {
        options.UseSqlServer(connectionString);
    }
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();

    var dbContext = scope.ServiceProvider.GetRequiredService<MediaStorageDbContext>();

    dbContext.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();



app.Run();
