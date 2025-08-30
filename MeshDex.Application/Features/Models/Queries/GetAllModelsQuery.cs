using System.Text.Json;
using MediatR;
using MeshDex.Domain.Entities;
using MeshDex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MeshDex.Application.Features.Models.Queries;

public sealed record GetAllModelsQuery : IRequest<List<LibraryModel>>;

internal sealed class GetAllModelsQueryHandler(ApplicationDbContext db)
    : IRequestHandler<GetAllModelsQuery, List<LibraryModel>>
{
    public async Task<List<LibraryModel>> Handle(GetAllModelsQuery request, CancellationToken cancellationToken)
    {
        var config = await db.LibraryConfigurations.FirstOrDefaultAsync(cancellationToken);
        var root = config?.RootDirectory?.Trim();
        if (string.IsNullOrWhiteSpace(root))
            throw new InvalidOperationException("Library root directory is not configured. Go to Settings → Configuration to set it.");

        var models = new List<LibraryModel>();
        foreach (var directory in Directory.EnumerateDirectories(root))
        {
            var metadataPath = Path.Combine(directory, "metadata.json");
            if (!File.Exists(metadataPath)) continue;
            try
            {
                var json = await File.ReadAllTextAsync(metadataPath, cancellationToken);
                var model = JsonSerializer.Deserialize<LibraryModel>(json);
                if (model != null)
                {
                    models.Add(model);
                }
            }
            catch
            {
                // ignore malformed model files, consistent with service behavior
            }
        }
        return models;
    }
}