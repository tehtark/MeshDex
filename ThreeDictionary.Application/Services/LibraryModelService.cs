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
        var root = config?.RootDirectory?.Trim();
        if (string.IsNullOrWhiteSpace(root))
            throw new InvalidOperationException("Library root directory is not configured. Go to Settings → Configuration to set it.");
        
        var directories = Directory.EnumerateDirectories(root);
        var models = new List<LibraryModel>();

        foreach (var directory in directories)
        {
            // Check if metadata.json exists in the directory
            var metadataPath = Path.Combine(directory, "metadata.json");
            if (File.Exists(metadataPath))
            {
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
        }

        return models;
    }
    public async Task CreateModelAsync(string name, int categoryId, CancellationToken ct = default)
    {
        // Validate inputs
        var trimmed = (name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            throw new InvalidOperationException("Model name is required.");

        // Ensure category exists
        var category = await libraryCategoryService.GetCategoryAsync(categoryId);
        if (category is null)
            throw new InvalidOperationException("Selected category was not found.");

        // Load configuration root
        var config = await libraryConfigurationService.GetConfigurationAsync();
        var root = config?.RootDirectory?.Trim();
        if (string.IsNullOrWhiteSpace(root))
            throw new InvalidOperationException("Library root directory is not configured. Go to Settings → Configuration to set it.");

        // Compute model folder safe path
        var modelFolderName = FileSystemNameHelper.MakeSafeFolderName(trimmed, fallback: "Model");
        var modelPath = Path.Combine(root, modelFolderName);

        // Check duplicates within category
        if (Directory.Exists(modelPath))
            throw new InvalidOperationException($"A model named '{trimmed}' already exists in this category.");

        LibraryModel model = new()
        {
            Name = name,
            CategoryId = categoryId,
            Path = modelPath
        };

        // Try create directory
        try
        {
            Directory.CreateDirectory(modelPath);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create model folder at '{modelPath}': {ex.Message}", ex);
        }

        //Create a metadata json file for the model.
        var jsonPath = Path.Combine(modelPath, "metadata.json");
        string json = JsonSerializer.Serialize(model, new JsonSerializerOptions() { WriteIndented = true });

        try
        {
            await File.WriteAllTextAsync(jsonPath, json, ct);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create metadata file at '{jsonPath}': {ex.Message}", ex);
        }
        
        await dbContext.LibraryModels.AddAsync(model, ct);
        await dbContext.SaveChangesAsync(ct);
    }
}