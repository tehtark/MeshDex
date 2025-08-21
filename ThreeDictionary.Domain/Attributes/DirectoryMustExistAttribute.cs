using System.ComponentModel.DataAnnotations;
using ThreeDictionary.Domain.Entities;

namespace ThreeDictionary.Domain.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class DirectoryMustExistAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var path = (value as string)?.Trim();

        if (string.IsNullOrWhiteSpace(path))
            return new ValidationResult("Directory path cannot be null or empty.");

        if (!Directory.Exists(path))
            return new ValidationResult("Directory does not exist.");

        return Directory.EnumerateFileSystemEntries(path).Any() ? new ValidationResult("The directory must be empty.") : ValidationResult.Success;
    }
}