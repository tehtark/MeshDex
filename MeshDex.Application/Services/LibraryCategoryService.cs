using MediatR;
using MeshDex.Application.Features.Categories.Queries;
using MeshDex.Application.Features.Categories.Commands;
using MeshDex.Domain.Entities;

namespace MeshDex.Application.Services;

public class LibraryCategoryService(IMediator mediator)
{
    public Task<List<LibraryCategory>> GetCategoriesAsync(CancellationToken cancellationToken = default)
        => mediator.Send(new GetCategoriesQuery(), cancellationToken);

    public Task<string?> GetCategoryNameAsync(int id, CancellationToken cancellationToken = default)
        => mediator.Send(new GetCategoryNameQuery(id), cancellationToken);
    
    public Task<LibraryCategory?> GetCategoryAsync(int id, CancellationToken cancellationToken = default)
        => mediator.Send(new GetCategoryQuery(id), cancellationToken);

    public Task<LibraryCategory> CreateCategoryAsync(LibraryCategory category, CancellationToken cancellationToken = default)
        => mediator.Send(new CreateCategoryCommand(category), cancellationToken);

    public Task<LibraryCategory> UpdateCategoryAsync(LibraryCategory category, CancellationToken cancellationToken = default)
        => mediator.Send(new UpdateCategoryCommand(category), cancellationToken);

    public async Task DeleteCategoryAsync(int id, CancellationToken cancellationToken = default)
    {
        await mediator.Send(new DeleteCategoryCommand(id), cancellationToken);
    }

    public Task<bool> CategoryExistsAsync(int id, CancellationToken cancellationToken = default)
        => mediator.Send(new CategoryExistsQuery(id), cancellationToken);

    public Task<bool> HasChildrenAsync(int id, CancellationToken cancellationToken = default)
        => mediator.Send(new CategoryHasChildrenQuery(id), cancellationToken);
}