using MediatR;
using MeshDex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace MeshDex.Application.Features.Configuration.Commands;

public sealed record ToggleInitialisedCommand : IRequest<bool>;

internal sealed class ToggleInitialisedCommandHandler(ApplicationDbContext db)
    : IRequestHandler<ToggleInitialisedCommand, bool>
{
    public async Task<bool> Handle(ToggleInitialisedCommand request, CancellationToken cancellationToken)
    {
        var configuration = await db.LibraryConfigurations.FirstOrDefaultAsync(cancellationToken);
        if (configuration == null) return false;

        configuration.Initialised = !configuration.Initialised;
        try
        {
            db.LibraryConfigurations.Update(configuration);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception e)
        {
            Log.Error(e, "{eMessage}", e.Message);
            return false;
        }
    }
}