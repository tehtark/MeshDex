using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ThreeDictionary.Domain.Entities;

namespace ThreeDictionary.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<User>(options)
{
    public DbSet<LibraryConfiguration> LibraryConfigurations { get; set; }
    public DbSet<LibraryCategory> LibraryCategories { get; set; }
    
    public DbSet<LibraryModel> LibraryModels { get; set; }
}