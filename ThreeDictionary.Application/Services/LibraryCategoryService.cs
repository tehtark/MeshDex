using Microsoft.EntityFrameworkCore;
using ThreeDictionary.Domain.Entities;
using ThreeDictionary.Infrastructure.Data;

namespace ThreeDictionary.Application.Services;

public class LibraryCategoryService(ApplicationDbContext dbContext)
{
    public async Task<List<LibraryCategory>> GetCategoriesAsync()
    {
        return await dbContext.LibraryCategories.ToListAsync();
    }

    public async Task<string?> GetCategoryNameAsync(int id)
    {
        return await dbContext.LibraryCategories
            .Where(c => c.Id == id)
            .Select(c => c.Name)
            .FirstOrDefaultAsync();
    }

    public async Task<LibraryCategory?> GetCategoryAsync(string name)
    {
        return await dbContext.LibraryCategories.FirstOrDefaultAsync(c => c.Name == name);
    }

    public async Task<LibraryCategory> CreateCategoryAsync(LibraryCategory category)
    {
        // Persist the category first
        dbContext.LibraryCategories.Add(category);
        await dbContext.SaveChangesAsync();

        // Then attempt to create the corresponding folder under the library root directory
        var config = await dbContext.LibraryConfigurations.FirstOrDefaultAsync();
        var root = config?.RootDirectory?.Trim();
        if (!string.IsNullOrWhiteSpace(root))
        {
            // Build a hierarchical path using the parent chain if present
            var categoryPath = await BuildCategoryFolderPathAsync(category, root);
            try
            {
                if (!string.IsNullOrWhiteSpace(categoryPath))
                {
                    Directory.CreateDirectory(categoryPath);
                }
            }
            catch (Exception ex)
            {
                // Surface the error to the caller so UI can show it
                throw new InvalidOperationException($"Failed to create category folder at '{categoryPath}': {ex.Message}", ex);
            }
        }

        return category;
    }

    public async Task<LibraryCategory> UpdateCategoryAsync(LibraryCategory category)
    {
        // Load existing entity
        var existing = await dbContext.LibraryCategories.FirstOrDefaultAsync(c => c.Id == category.Id);
        if (existing == null)
            throw new InvalidOperationException($"Category with id {category.Id} was not found.");

        // Prevent setting parent to self
        if (category.ParentId == category.Id)
            throw new InvalidOperationException("A category cannot be its own parent.");

        // Prevent cycles: walk up from the proposed parent to ensure we don't reach this category
        if (await WouldCreateCycleAsync(category.Id, category.ParentId))
            throw new InvalidOperationException("Invalid parent selection: it would create a circular hierarchy.");

        // Compute filesystem paths
        var config = await dbContext.LibraryConfigurations.FirstOrDefaultAsync();
        var root = config?.RootDirectory?.Trim();

        string? oldPath = null;
        string? newPath = null;

        if (!string.IsNullOrWhiteSpace(root))
        {
            // Old path using current stored values
            oldPath = await BuildCategoryFolderPathAsync(existing, root);

            // New path using proposed values (without persisting yet)
            var temp = new LibraryCategory { Id = existing.Id, Name = category.Name, ParentId = category.ParentId };
            newPath = await BuildCategoryFolderPathAsync(temp, root);
        }

        // If we have a filesystem context and the path changed, move/rename
        if (!string.IsNullOrWhiteSpace(oldPath) && !string.IsNullOrWhiteSpace(newPath))
        {
            var pathChanged = !string.Equals(oldPath, newPath, StringComparison.OrdinalIgnoreCase);
            if (pathChanged)
            {
                try
                {
                    var newParentDir = Path.GetDirectoryName(newPath);
                    if (!string.IsNullOrWhiteSpace(newParentDir))
                        Directory.CreateDirectory(newParentDir);

                    if (Directory.Exists(oldPath))
                    {
                        if (Directory.Exists(newPath))
                        {
                            throw new InvalidOperationException($"Cannot move category folder: destination already exists at '{newPath}'.");
                        }
                        // Perform move (includes subtree)
                        Directory.Move(oldPath, newPath);
                    }
                    else
                    {
                        // Old path missing: create the new path to keep FS in sync
                        Directory.CreateDirectory(newPath);
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to move category folder from '{oldPath}' to '{newPath}': {ex.Message}", ex);
                }
            }
            else
            {
                // Handle case-only rename on case-insensitive FS (optional): try normalize case if needed
                if (!string.Equals(oldPath, newPath, StringComparison.Ordinal))
                {
                    try
                    {
                        var tmp = newPath + ".tmp_case_rename";
                        if (Directory.Exists(oldPath))
                        {
                            Directory.Move(oldPath, tmp);
                            Directory.Move(tmp, newPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Non-fatal; log surface as warning if needed
                        Console.WriteLine(ex);
                    }
                }
            }
        }

        // Persist DB changes only after filesystem operations succeed
        existing.Name = category.Name;
        existing.ParentId = category.ParentId;

        dbContext.LibraryCategories.Update(existing);
        await dbContext.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteCategoryAsync(int id)
    {
        var category = await dbContext.LibraryCategories.FindAsync(id);
        if (category != null)
        {
            // Recursively delete all child categories first
            var children = await dbContext.LibraryCategories.Where(c => c.ParentId == id).ToListAsync();
            foreach (var child in children)
            {
                // recursion ensures deep tree deletion
                await DeleteCategoryAsync(child.Id);
            }

            // Attempt to delete the corresponding folder (including nested content)
            var config = await dbContext.LibraryConfigurations.FirstOrDefaultAsync();
            var root = config?.RootDirectory?.Trim();
            if (!string.IsNullOrWhiteSpace(root))
            {
                var categoryPath = await BuildCategoryFolderPathAsync(category, root);
                try
                {
                    if (!string.IsNullOrWhiteSpace(categoryPath) && Directory.Exists(categoryPath))
                    {
                        Directory.Delete(categoryPath, recursive: true);
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to delete category folder at '{categoryPath}': {ex.Message}", ex);
                }
            }

            // Only remove from DB if folder deletion succeeded or no folder existed
            dbContext.LibraryCategories.Remove(category);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task DeleteCategoryAsync(string name)
    {
        var category = await dbContext.LibraryCategories.FirstOrDefaultAsync(c => c.Name == name);
        if (category != null)
        {
            // Delegate to id-based deletion for recursive handling
            await DeleteCategoryAsync(category.Id);
        }
    }

    public async Task<bool> CategoryExistsAsync(int id)
    {
        return await dbContext.LibraryCategories.AnyAsync(c => c.Id == id);
    }

    public async Task<bool> CategoryExistsAsync(string name)
    {
        return await dbContext.LibraryCategories.AnyAsync(c => c.Name == name);
    }

    public async Task<bool> HasChildrenAsync(int id)
    {
        return await dbContext.LibraryCategories.AnyAsync(c => c.ParentId == id);
    }

    private async Task<bool> WouldCreateCycleAsync(int categoryId, int? proposedParentId)
    {
        if (proposedParentId is null) return false;
        if (proposedParentId == categoryId) return true;

        var visited = new HashSet<int>();
        var currentId = proposedParentId;
        while (currentId is int pid)
        {
            if (!visited.Add(pid)) break; // safeguard against unexpected loops
            if (pid == categoryId) return true;
            var parent = await dbContext.LibraryCategories.FirstOrDefaultAsync(c => c.Id == pid);
            if (parent == null) break;
            currentId = parent.ParentId;
        }
        return false;
    }

    private static string MakeSafeFolderName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string(name.Where(ch => !invalid.Contains(ch)).ToArray()).Trim();
        if (string.IsNullOrWhiteSpace(cleaned))
        {
            // Fallback: use a default pattern if name becomes empty after cleaning
            cleaned = "Category";
        }
        // Optionally normalize spaces
        while (cleaned.Contains("  ")) cleaned = cleaned.Replace("  ", " ");
        // Replace remaining spaces with underscores for filesystem friendliness
        cleaned = cleaned.Replace(' ', '_');
        return cleaned;
    }

    private async Task<string> BuildCategoryFolderPathAsync(LibraryCategory category, string root)
    {
        // Build ancestor chain from topmost parent to current category
        var segments = new List<string>();
        // Walk up the tree collecting parents first
        var current = category;
        // For newly added categories, ParentId may refer to an existing tracked entity
        while (current?.ParentId is int pid)
        {
            var parent = await dbContext.LibraryCategories.FirstOrDefaultAsync(c => c.Id == pid);
            if (parent == null) break;
            segments.Insert(0, MakeSafeFolderName(parent.Name));
            current = parent;
        }
        // Finally add this category's own safe name
        segments.Add(MakeSafeFolderName(category.Name));
        return Path.Combine(new[] { root }.Concat(segments).ToArray());
    }
}