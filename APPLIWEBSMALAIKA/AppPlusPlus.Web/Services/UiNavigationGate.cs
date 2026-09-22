namespace AppPlusPlus.Web.Services;

/// <summary>Empêche les navigations / actions UI concurrentes (double-clic) sur Blazor Server.</summary>
public sealed class UiNavigationGate
{
    int _locks;

    public bool IsLocked => Volatile.Read(ref _locks) > 0;

    public bool TryEnter() => Interlocked.Increment(ref _locks) == 1;

    public void Exit()
    {
        if (Volatile.Read(ref _locks) > 0)
            Interlocked.Decrement(ref _locks);
    }

    public void ForceReset() => Interlocked.Exchange(ref _locks, 0);
}
