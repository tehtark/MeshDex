namespace ThreeDictionary.Services;

public class FileService
{
    public FileService()
    {
        if (Directory.Exists("C:/"))
            DefaultPath = "C:/";
        else if (Directory.Exists("D:/"))
            DefaultPath = "D:/";
        else
            DefaultPath = AppDomain.CurrentDomain.BaseDirectory;
    }

    public string DefaultPath { get; set; }

    public List<string?>? GetFiles(string path)
    {
        if (Path.Exists(path))
        {
            var files = Directory.GetFiles(path);
            return files.Select(Path.GetFileName).ToList();
        }

        return null;
    }

    public List<string?>? GetDirectories(string path)
    {
        if (Directory.Exists(path)) return Directory.GetDirectories(path).Select(Path.GetFileName).ToList();

        return null;
    }
}