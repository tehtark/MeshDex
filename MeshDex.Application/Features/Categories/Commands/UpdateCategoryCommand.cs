using MediatR;
using MeshDex.Domain.Entities;
using MeshDex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MeshDex.Application.Features.Categories.Commands;
public sealed record UpdateCategoryCommand(LibraryCategory Category) : IRequest<LibraryCategory>;

internal sealed class UpdateCategoryCommandHandler(ApplicationDbContext db)
    : IRequestHandler<UpdateCategoryCommand, LibraryCategory>
{
    public async Task<LibraryCategory> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = request.Category;
        var existing = await db.LibraryCategories.FirstOrDefaultAsync(c => c.Id == category.Id, cancellationToken);
        if (existing == null)
            throw new InvalidOperationException($"Category with id {category.Id} was not found.");

        if (category.ParentId == category.Id)
            throw new InvalidOperationException("A category cannot be its own parent.");

        if (await WouldCreateCycleAsync(category.Id, category.ParentId, cancellationToken))
            throw new InvalidOperationException("Invalid parent selection: it would create a circular hierarchy.");

        existing.Name = category.Name;
        existing.ParentId = category.ParentId;
        db.LibraryCategories.Update(existing);
        await db.SaveChangesAsync(cancellationToken);
        return existing;
    }

    private async Task<bool> WouldCreateCycleAsync(int categoryId, int? proposedParentId, CancellationToken ct)
    {
        if (proposedParentId is null) return false;
        if (proposedParentId == categoryId) return true;

        var visited = new HashSet<int>();
        var currentId = proposedParentId;
        while (currentId is int pid)
        {
            if (!visited.Add(pid)) break;
            if (pid == categoryId) return true;
            var parent = await db.LibraryCategories.FirstOrDefaultAsync(c => c.Id == pid, ct);
            if (parent == null) break;
            currentId = parent.ParentId;
        }
        return false;
    }
}
