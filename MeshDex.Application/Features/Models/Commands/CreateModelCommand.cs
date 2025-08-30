using System.Text.Json;
using MediatR;
using MeshDex.Application.Utils;
using MeshDex.Domain.Entities;
using MeshDex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MeshDex.Application.Features.Models.Commands;

public sealed record CreateModelCommand(string? Name, int CategoryId) : IRequest<Unit>;

internal sealed class CreateModelCommandHandler(ApplicationDbContext db)
    : IRequestHandler<CreateModelCommand, Unit>
{
    public async Task<Unit> Handle(CreateModelCommand request, CancellationToken ct)
    {
        var trimmed = (request.Name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            throw new InvalidOperationException("Model name is required.");

        var category = await db.LibraryCategories.FirstOrDefaultAsync(c => c.Id == request.CategoryId, ct);
        if (category is null)
            throw new InvalidOperationException("Selected category was not found.");

        var config = await db.LibraryConfigurations.FirstOrDefaultAsync(ct);
        var root = config?.RootDirectory?.Trim();
        if (string.IsNullOrWhiteSpace(root))
            throw new InvalidOperationException("Library root directory is not configured. Go to Settings → Configuration to set it.");

        root = Path.GetFullPath(root);

        var modelFolderName = FileSystemNameHelper.MakeSafeFolderName(trimmed, fallback: "Model");
        var modelPath = Path.Combine(root, modelFolderName);
        var modelPathFull = Path.GetFullPath(modelPath);

        if (Directory.Exists(modelPath))
            throw new InvalidOperationException($"A model named '{trimmed}' already exists in this category.");

        var rel = Path.GetRelativePath(root, modelPathFull);
        if (rel.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal) || Path.IsPathRooted(rel))
            throw new InvalidOperationException("Resolved path escapes the library root.");

        try
        {
            Directory.CreateDirectory(modelPathFull);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create model folder at '{modelPath}': {ex.Message}", ex);
        }

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var model = new LibraryModel()
        {
            Name = trimmed,
            CategoryId = request.CategoryId,
            Path = rel.Replace(Path.DirectorySeparatorChar, '/'),
        };

        try
        {
            await db.LibraryModels.AddAsync(model, ct);
            await db.SaveChangesAsync(ct);

            var meta = new
            {
                SchemaVersion = 1,
                model.Id,
                model.Name,
                model.CategoryId,
                model.Path,
                CreatedUtc = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(meta, new JsonSerializerOptions() { WriteIndented = true });
            var jsonPath = Path.Combine(modelPath, "metadata.json");
            var tmpPath = jsonPath + ".tmp";

            await File.WriteAllTextAsync(tmpPath, json, ct);

            if (File.Exists(jsonPath))
            {
                File.Replace(tmpPath, jsonPath, null);
            }
            else
            {
                File.Move(tmpPath, jsonPath);
            }

            await tx.CommitAsync(ct);
        }
        catch (Exception)
        {
            try
            {
                if (Directory.Exists(modelPathFull) &&
                    !Directory.EnumerateFileSystemEntries(modelPathFull).Any())
                {
                    Directory.Delete(modelPathFull);
                }
            }
            catch { }
            await tx.RollbackAsync(ct);
            throw;
        }

        return Unit.Value;
    }
}