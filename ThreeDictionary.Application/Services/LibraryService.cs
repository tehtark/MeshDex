using System.Runtime.InteropServices.ComTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using ThreeDictionary.Domain.Entities;
using ThreeDictionary.Infrastructure.Data;

namespace ThreeDictionary.Application.Services;

public class LibraryService(ApplicationDbContext dbContext) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var configuration = await GetConfigurationAsync();

        while (!stoppingToken.IsCancellationRequested && string.IsNullOrEmpty(configuration?.RootDirectory))
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    public async Task<LibraryConfiguration?> GetConfigurationAsync()
    {
        return await dbContext.LibraryConfigurations.FirstOrDefaultAsync();
    }

    public async Task<bool> UpdateRootDirectory(string path)
    {
        var configuration = await GetConfigurationAsync();
        if (configuration == null) return false;
        configuration.RootDirectory = path;
        try
        {
            dbContext.LibraryConfigurations.Update(configuration);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }

        return true;
    }

    public async Task<bool> ToggleInitialised()
    {
        var configuration = await GetConfigurationAsync();
        if (configuration == null) return false;

        configuration.Initialised = !configuration.Initialised;
        try
        {
            dbContext.LibraryConfigurations.Update(configuration);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }

        return true;
    }
}