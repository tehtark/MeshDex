using MediatR;
using MeshDex.Domain.Entities;
using MeshDex.Infrastructure.Data;

namespace MeshDex.Application.Features.Categories.Commands;

public sealed record CreateCategoryCommand(LibraryCategory Category) : IRequest<LibraryCategory>;

internal sealed class CreateCategoryCommandHandler(ApplicationDbContext db)
    : IRequestHandler<CreateCategoryCommand, LibraryCategory>
{
    public async Task<LibraryCategory> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        db.LibraryCategories.Add(request.Category);
        await db.SaveChangesAsync(cancellationToken);
        return request.Category;
    }
}
