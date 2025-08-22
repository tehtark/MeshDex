using System.Text.Json;
using ThreeDictionary.Infrastructure.Data;
using ThreeDictionary.Application.Utils;
using ThreeDictionary.Domain.Entities;

namespace ThreeDictionary.Application.Services;

public class LibraryModelService(
    ApplicationDbContext dbContext,
    LibraryConfigurationService libraryConfigurationService,
    LibraryCategoryService libraryCategoryService)
{
    public async Task<List<LibraryModel>> GetAllModelsAsync()
    {
        // Load configuration root
        var config = await libraryConfigurationService.GetConfigurationAsync();
        var root = config?.RootDirectory.Trim();
        if (string.IsNullOrWhiteSpace(root))
            throw new InvalidOperationException("Library root directory is not configured. Go to Settings → Configuration to set it.");

        var directories = Directory.EnumerateDirectories(root);
        var models = new List<LibraryModel>();

        foreach (var directory in directories)
        {
            // Check if metadata.json exists in the directory
            var metadataPath = Path.Combine(directory, "metadata.json");
            if (!File.Exists(metadataPath)) continue;
            try
            {
                // Read and deserialize the metadata file
                var json = await File.ReadAllTextAsync(metadataPath);
                var model = JsonSerializer.Deserialize<LibraryModel>(json);
                if (model != null)
                {
                    models.Add(model);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to read model from {metadataPath}: {ex.Message}");
            }
        }

        return models;
    }

    public async Task CreateModelAsync(string? name, int categoryId, CancellationToken ct = default)
    {
        // Validate inputs
        var trimmed = (name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            throw new InvalidOperationException("Model name is required.");

        // Ensure a category exists
        var category = await libraryCategoryService.GetCategoryAsync(categoryId);
        if (category is null)
            throw new InvalidOperationException("Selected category was not found.");

        // Load configuration root
        var config = await libraryConfigurationService.GetConfigurationAsync();
        var root = config?.RootDirectory.Trim();
        if (string.IsNullOrWhiteSpace(root))
            throw new InvalidOperationException("Library root directory is not configured. Go to Settings → Configuration to set it.");

        root = Path.GetFullPath(root);

        // Compute model folder safe path
        var modelFolderName = FileSystemNameHelper.MakeSafeFolderName(trimmed, fallback: "Model");
        var modelPath = Path.Combine(root, modelFolderName);
        var modelPathFull = Path.GetFullPath(modelPath);

        // Check duplicates within a category
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

        await using var tx = await dbContext.Database.BeginTransactionAsync(ct);
        var model = new LibraryModel()
        {
            Name = trimmed,
            CategoryId = categoryId,
            Path = rel.Replace(Path.DirectorySeparatorChar, '/'),
        };

        try
        {
            await dbContext.LibraryModels.AddAsync(model, ct);
            await dbContext.SaveChangesAsync(ct);

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
            // Try to roll back FS change if the directory is still empty
            try
            {
                if (Directory.Exists(modelPathFull) &&
                    !Directory.EnumerateFileSystemEntries(modelPathFull).Any())
                {
                    Directory.Delete(modelPathFull);
                }
            }
            catch { /* best effort clean-up */ }
            await tx.RollbackAsync(ct);
            throw;
        }
    }
}