using MediatR;
using MeshDex.Application.Features.Configuration.Commands;
using MeshDex.Application.Features.Configuration.Queries;
using MeshDex.Domain.Entities;

namespace MeshDex.Application.Services;

public class LibraryConfigurationService(IMediator mediator)
{
    public Task<LibraryConfiguration?> GetConfigurationAsync(CancellationToken cancellationToken = default)
        => mediator.Send(new GetLibraryConfigurationQuery(), cancellationToken);

    public Task<bool> UpdateRootDirectory(string path, CancellationToken cancellationToken = default)
        => mediator.Send(new UpdateRootDirectoryCommand(path), cancellationToken);

    public Task<bool> ToggleInitialised(CancellationToken cancellationToken = default)
        => mediator.Send(new ToggleInitialisedCommand(), cancellationToken);
}