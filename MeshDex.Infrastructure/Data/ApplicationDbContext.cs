using MeshDex.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MeshDex.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<User>(options)
{
    public DbSet<LibraryConfiguration> LibraryConfigurations { get; set; }
    public DbSet<LibraryCategory> LibraryCategories { get; set; }
    
    public DbSet<LibraryModel> LibraryModels { get; set; }
}