using MediatR;
using MeshDex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MeshDex.Application.Features.Categories.Queries;

public sealed record CategoryHasChildrenQuery(int Id) : IRequest<bool>;

internal sealed class CategoryHasChildrenQueryHandler(ApplicationDbContext db)
    : IRequestHandler<CategoryHasChildrenQuery, bool>
{
    public Task<bool> Handle(CategoryHasChildrenQuery request, CancellationToken cancellationToken)
        => db.LibraryCategories.AnyAsync(c => c.ParentId == request.Id, cancellationToken);
}
