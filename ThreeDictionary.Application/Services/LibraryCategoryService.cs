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
    
    public async Task<LibraryCategory?> GetCategoryAsync(int id)
    {
        return await dbContext.LibraryCategories.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<LibraryCategory> CreateCategoryAsync(LibraryCategory category)
    {
        dbContext.LibraryCategories.Add(category);
        await dbContext.SaveChangesAsync();
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

            // Only remove from DB if folder deletion succeeded or no folder existed
            dbContext.LibraryCategories.Remove(category);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task<bool> CategoryExistsAsync(int id)
    {
        return await dbContext.LibraryCategories.AnyAsync(c => c.Id == id);
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
}