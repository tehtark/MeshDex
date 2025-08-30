using MediatR;
using MeshDex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MeshDex.Application.Features.Categories.Queries;

public sealed record CategoryExistsQuery(int Id) : IRequest<bool>;

internal sealed class CategoryExistsQueryHandler(ApplicationDbContext db)
    : IRequestHandler<CategoryExistsQuery, bool>
{
    public Task<bool> Handle(CategoryExistsQuery request, CancellationToken cancellationToken)
        => db.LibraryCategories.AnyAsync(c => c.Id == request.Id, cancellationToken);
}
