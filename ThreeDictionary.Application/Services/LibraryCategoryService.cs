using Microsoft.EntityFrameworkCore;
using ThreeDictionary.Domain.Entities;
using ThreeDictionary.Infrastructure.Data;

namespace ThreeDictionary.Application.Services;

public class LibraryCategoryService(ApplicationDbContext dbContext)
{
    public async Task<List<LibraryCategory?>> GetCategoriesAsync()
    {
        return await dbContext.LibraryCategories.ToListAsync();
    }

    public async Task<LibraryCategory?> GetCategoryAsync(int id)
    {
        return await dbContext.LibraryCategories.FirstOrDefaultAsync(c => c != null && c.Id == id);
    }

    public async Task<LibraryCategory?> GetCategoryAsync(string name)
    {
        return await dbContext.LibraryCategories.FirstOrDefaultAsync(c => c != null && c.Name == name);
    }
    
    public async Task<LibraryCategory> CreateCategoryAsync(LibraryCategory category)
    {
        dbContext.LibraryCategories.Add(category);
        await dbContext.SaveChangesAsync();
        return category;
    }
    
    public async Task<LibraryCategory> UpdateCategoryAsync(LibraryCategory category)
    {
        dbContext.LibraryCategories.Update(category);
        await dbContext.SaveChangesAsync();
        return category;
    }
    
    public async Task DeleteCategoryAsync(int id)
    {
        var category = await dbContext.LibraryCategories.FindAsync(id);
        if (category != null)
        {
            dbContext.LibraryCategories.Remove(category);
            await dbContext.SaveChangesAsync();
        }
    }
    public async Task DeleteCategoryAsync(string name)
    {
        var category = await dbContext.LibraryCategories.FirstOrDefaultAsync(c => c != null && c.Name == name);
        if (category != null)
        {
            dbContext.LibraryCategories.Remove(category);
            await dbContext.SaveChangesAsync();
        }
    }
    public async Task<bool> CategoryExistsAsync(int id)
    {
        return await dbContext.LibraryCategories.AnyAsync(c => c != null && c.Id == id);
    }
    public async Task<bool> CategoryExistsAsync(string name)
    {
        return await dbContext.LibraryCategories.AnyAsync(c => c != null && c.Name == name);
    }
    
}