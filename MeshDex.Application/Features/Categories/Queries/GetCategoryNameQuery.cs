using MediatR;
using MeshDex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MeshDex.Application.Features.Categories.Queries;

public sealed record GetCategoryNameQuery(int Id) : IRequest<string?>;

internal sealed class GetCategoryNameQueryHandler(ApplicationDbContext db)
    : IRequestHandler<GetCategoryNameQuery, string?>
{
    public async Task<string?> Handle(GetCategoryNameQuery request, CancellationToken cancellationToken)
    {
        return await db.LibraryCategories
            .Where(c => c.Id == request.Id)
            .Select(c => c.Name)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
