using MediatR;
using MeshDex.Application.Features.Categories.Queries;
using MeshDex.Application.Features.Categories.Commands;
using MeshDex.Domain.Entities;

namespace MeshDex.Application.Services;

public class LibraryCategoryService(IMediator mediator)
{
    public Task<List<LibraryCategory>> GetCategoriesAsync()
        => mediator.Send(new GetCategoriesQuery());

    public Task<string?> GetCategoryNameAsync(int id)
        => mediator.Send(new GetCategoryNameQuery(id));
    
    public Task<LibraryCategory?> GetCategoryAsync(int id)
        => mediator.Send(new GetCategoryQuery(id));

    public Task<LibraryCategory> CreateCategoryAsync(LibraryCategory category)
        => mediator.Send(new CreateCategoryCommand(category));

    public Task<LibraryCategory> UpdateCategoryAsync(LibraryCategory category)
        => mediator.Send(new UpdateCategoryCommand(category));

    public async Task DeleteCategoryAsync(int id)
    {
        await mediator.Send(new DeleteCategoryCommand(id));
    }

    public Task<bool> CategoryExistsAsync(int id)
        => mediator.Send(new CategoryExistsQuery(id));

    public Task<bool> HasChildrenAsync(int id)
        => mediator.Send(new CategoryHasChildrenQuery(id));
}