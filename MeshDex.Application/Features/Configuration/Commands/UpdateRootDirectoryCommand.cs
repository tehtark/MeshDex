using MediatR;
using MeshDex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace MeshDex.Application.Features.Configuration.Commands;

public sealed record UpdateRootDirectoryCommand(string Path) : IRequest<bool>;

internal sealed class UpdateRootDirectoryCommandHandler(ApplicationDbContext db)
    : IRequestHandler<UpdateRootDirectoryCommand, bool>
{
    public async Task<bool> Handle(UpdateRootDirectoryCommand request, CancellationToken cancellationToken)
    {
        var configuration = await db.LibraryConfigurations.FirstOrDefaultAsync(cancellationToken);
        if (configuration == null) return false;
        configuration.RootDirectory = request.Path;
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