using MediatR;
using MeshDex.Domain.Entities;
using MeshDex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MeshDex.Application.Features.Configuration.Queries;

public sealed record GetLibraryConfigurationQuery : IRequest<LibraryConfiguration?>;

internal sealed class GetLibraryConfigurationQueryHandler(ApplicationDbContext db)
    : IRequestHandler<GetLibraryConfigurationQuery, LibraryConfiguration?>
{
    public async Task<LibraryConfiguration?> Handle(GetLibraryConfigurationQuery request, CancellationToken cancellationToken)
    {
        return await db.LibraryConfigurations.FirstOrDefaultAsync(cancellationToken);
    }
}