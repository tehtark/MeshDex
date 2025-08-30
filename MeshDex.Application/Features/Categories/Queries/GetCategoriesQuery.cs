using MediatR;
using MeshDex.Domain.Entities;
using MeshDex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MeshDex.Application.Features.Categories.Queries;

public sealed record GetCategoriesQuery : IRequest<List<LibraryCategory>>;

internal sealed class GetCategoriesQueryHandler(ApplicationDbContext db)
    : IRequestHandler<GetCategoriesQuery, List<LibraryCategory>>
{
    public async Task<List<LibraryCategory>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        return await db.LibraryCategories.ToListAsync(cancellationToken);
    }
}
