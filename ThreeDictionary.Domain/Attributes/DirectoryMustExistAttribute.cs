using System.ComponentModel.DataAnnotations;

namespace ThreeDictionary.Domain.Attributes;

public class DirectoryMustExistAttribute: ValidationAttribute
{
    public DirectoryMustExistAttribute(string path)
    {
        Path = path;
    }

    public string Path { get; }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var directoryPath = (string)validationContext.ObjectInstance;
        
        if (string.IsNullOrEmpty(directoryPath) || !Directory.Exists(directoryPath))
        {
            return new ValidationResult("The specified directory does not exist.");
        }
        


        return ValidationResult.Success;
    }
}
