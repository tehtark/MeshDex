using System.ComponentModel.DataAnnotations;
using ThreeDictionary.Domain.Attributes;

namespace ThreeDictionary.Domain.Entities;

public class LibraryConfiguration
{
    [Key] [Required] public int Id { get; set; } = 1;
    [Required] public bool Initialised { get; set; }
    
    [Required] 
    [DirectoryMustExist]
    public string RootDirectory { get; set; } = string.Empty;
}