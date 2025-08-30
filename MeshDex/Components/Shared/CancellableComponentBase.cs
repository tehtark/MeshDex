using Microsoft.AspNetCore.Components;

namespace MeshDex.Components.Shared;

public abstract class CancellableComponentBase : ComponentBase, IAsyncDisposable
{
    private CancellationTokenSource? _cts;

    protected CancellationToken NewCts()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        return _cts.Token;
    }

    protected void CancelOngoing()
    {
        _cts?.Cancel();
    }

    public ValueTask DisposeAsync()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        return ValueTask.CompletedTask;
    }
}