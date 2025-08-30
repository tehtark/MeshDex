using MediatR;
using MeshDex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MeshDex.Application.Features.Categories.Commands;

public sealed record DeleteCategoryCommand(int Id) : IRequest<Unit>;

internal sealed class DeleteCategoryCommandHandler(ApplicationDbContext db)
    : IRequestHandler<DeleteCategoryCommand, Unit>
{
    public async Task<Unit> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        await DeleteRecursiveAsync(request.Id, cancellationToken);
        return Unit.Value;
    }

    /// <summary>
    /// Recursively deletes a category and its child categories from the database.
    /// </summary>
    /// <param name="id">The ID of the category to be deleted.</param>
    /// <param name="ct">The cancellation token to propagate notifications that the operation should be canceled.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    private async Task DeleteRecursiveAsync(int id, CancellationToken ct)
    {
        var category = await db.LibraryCategories.FindAsync([id], ct);
        if (category == null) return;

        var children = await db.LibraryCategories.Where(c => c.ParentId == id).ToListAsync(ct);
        foreach (var child in children)
        {
            await DeleteRecursiveAsync(child.Id, ct);
        }

        db.LibraryCategories.Remove(category);
        await db.SaveChangesAsync(ct);
    }
}
