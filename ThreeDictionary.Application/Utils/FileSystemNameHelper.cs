namespace ThreeDictionary.Application.Utils;

public static class FileSystemNameHelper
{
    public static string MakeSafeFolderName(string name)
    {
        return MakeSafeFolderName(name, fallback: "Category");
    }

    public static string MakeSafeFolderName(string name, string fallback)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string((name ?? string.Empty).Where(ch => !invalid.Contains(ch)).ToArray()).Trim();
        if (string.IsNullOrWhiteSpace(cleaned)) cleaned = fallback;
        // Normalize multiple spaces
        while (cleaned.Contains("  ")) cleaned = cleaned.Replace("  ", " ");
        // Replace spaces with underscores for filesystem friendliness
        cleaned = cleaned.Replace(' ', '_');
        return cleaned;
    }
}