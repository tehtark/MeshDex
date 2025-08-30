using MediatR;
using MeshDex.Domain.Entities;
using MeshDex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MeshDex.Application.Features.Categories.Queries;

public sealed record GetCategoryQuery(int Id) : IRequest<LibraryCategory?>;

internal sealed class GetCategoryQueryHandler(ApplicationDbContext db)
    : IRequestHandler<GetCategoryQuery, LibraryCategory?>
{
    public Task<LibraryCategory?> Handle(GetCategoryQuery request, CancellationToken cancellationToken)
        => db.LibraryCategories.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
}
