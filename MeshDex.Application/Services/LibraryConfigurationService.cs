using MediatR;
using MeshDex.Application.Features.Configuration.Commands;
using MeshDex.Application.Features.Configuration.Queries;
using MeshDex.Domain.Entities;

namespace MeshDex.Application.Services;

public class LibraryConfigurationService(IMediator mediator)
{
    public Task<LibraryConfiguration?> GetConfigurationAsync()
        => mediator.Send(new GetLibraryConfigurationQuery());

    public Task<bool> UpdateRootDirectory(string path)
        => mediator.Send(new UpdateRootDirectoryCommand(path));

    public Task<bool> ToggleInitialised()
        => mediator.Send(new ToggleInitialisedCommand());
}