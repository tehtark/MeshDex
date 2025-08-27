using System.ComponentModel.DataAnnotations;

namespace MeshDex.Domain.Entities;

public class LibraryModel
{
    [Key] [Required]public int Id { get; set; }
    [Required]public string Name { get; set; } = string.Empty;
    [Required]public int CategoryId { get; set; }
    [Required] public string Path { get; set; } = string.Empty;
}