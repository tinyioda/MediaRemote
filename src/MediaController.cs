using Windows.Media.Control;

namespace MediaRemote;

/// <summary>
/// Drives whichever app currently owns the system media session (Spotify, a browser
/// tab, etc.) via the real WinRT session API, instead of faking media keypresses.
/// </summary>
internal static class MediaController
{
    private static GlobalSystemMediaTransportControlsSessionManager? _manager;

    private static async Task<GlobalSystemMediaTransportControlsSessionManager> GetManagerAsync()
        => _manager ??= await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();

    public static async Task PlayPauseAsync() => await WithCurrentSessionAsync(s => s.TryTogglePlayPauseAsync().AsTask());

    public static async Task NextAsync() => await WithCurrentSessionAsync(s => s.TrySkipNextAsync().AsTask());

    public static async Task PreviousAsync() => await WithCurrentSessionAsync(s => s.TrySkipPreviousAsync().AsTask());

    private static async Task WithCurrentSessionAsync(Func<GlobalSystemMediaTransportControlsSession, Task> action)
    {
        var manager = await GetManagerAsync();
        var session = manager.GetCurrentSession();
        if (session is not null)
        {
            await action(session);
        }
    }
}
