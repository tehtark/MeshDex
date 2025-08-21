using System.ComponentModel.DataAnnotations;

namespace ThreeDictionary.Domain.Entities;

public class LibraryCategory
{
    [Key] [Required] public int Id { get; set; }
    [Required] public string Name { get; set; } = string.Empty;
    public int? ParentId { get; set; }
}