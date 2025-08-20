namespace ThreeDictionary.Services;

public class LibraryService : BackgroundService
{
    public string Path { get; private set; } = "";
 
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // This method is called when the service starts.
        // I will need to pull the path from the configuration or a settings file.

        while (!stoppingToken.IsCancellationRequested && string.IsNullOrEmpty(Globals.LibraryPath))
        {
            await Task.Delay(1000, stoppingToken);
            
            // Check if the path is set in the configuration or settings file
            if (!string.IsNullOrEmpty(Globals.LibraryPath))
            {
                Path = Globals.LibraryPath;
            }
        }
        
        // I should 
    }
    
}