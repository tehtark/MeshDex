using System.ComponentModel.DataAnnotations;
using MeshDex.Domain.Attributes;

namespace MeshDex.Domain.Entities;

public class LibraryConfiguration
{
    [Key] [Required] public int Id { get; set; } = 1;
    [Required] public bool Initialised { get; set; }

    [Required] [DirectoryMustExist] public string RootDirectory { get; set; } = string.Empty;
}