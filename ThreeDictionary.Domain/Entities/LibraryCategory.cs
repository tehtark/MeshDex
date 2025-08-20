namespace ThreeDictionary.Domain.Entities;

public class LibraryCategory
{
    public int? ParentId { get; set; }
    public int? ChildId { get; set; }
    public int Id { get; set; }
    public string Name { get; set; }
}