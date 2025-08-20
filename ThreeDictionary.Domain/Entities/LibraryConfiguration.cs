using System.ComponentModel.DataAnnotations;

namespace ThreeDictionary.Domain.Entities;

public class LibraryConfiguration
{
    [Key] [Required] public int Id { get; set; } = 1;
    [Required] public bool Initialised { get; set; }
    [Required] public string RootDirectory { get; set; } = string.Empty;
}