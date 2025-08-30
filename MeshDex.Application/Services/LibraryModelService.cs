using MediatR;
using MeshDex.Application.Features.Models.Commands;
using MeshDex.Application.Features.Models.Queries;
using MeshDex.Domain.Entities;

namespace MeshDex.Application.Services;

public class LibraryModelService(IMediator mediator)
{
    public Task<List<LibraryModel>> GetAllModelsAsync(CancellationToken ct = default)
        => mediator.Send(new GetAllModelsQuery(), ct);

    public async Task CreateModelAsync(string? name, int categoryId, CancellationToken ct = default)
    {
        await mediator.Send(new CreateModelCommand(name, categoryId), ct);
    }
}