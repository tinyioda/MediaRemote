using Windows.Gaming.Input;

namespace MediaRemote;

/// <summary>
/// Polls the first connected Xbox controller and maps a few buttons to media/volume
/// actions. Button mapping, change here if you want different controls:
///   D-Pad Left  -> previous track
///   D-Pad Right -> next track
///   D-Pad Up    -> volume up
///   D-Pad Down  -> volume down
///   A           -> play/pause
/// </summary>
internal sealed class GamepadService : IDisposable
{
    private readonly System.Windows.Forms.Timer _timer;
    private GamepadButtons _previousButtons = GamepadButtons.None;
    private bool _busy;

    public bool Enabled { get; set; } = true;

    public GamepadService()
    {
        _timer = new System.Windows.Forms.Timer { Interval = 50 };
        _timer.Tick += async (_, _) => await TickAsync();
        _timer.Start();
    }

    private async Task TickAsync()
    {
        if (_busy)
        {
            return;
        }

        var gamepad = Gamepad.Gamepads.FirstOrDefault();
        if (gamepad is null)
        {
            _previousButtons = GamepadButtons.None;
            return;
        }

        var buttons = gamepad.GetCurrentReading().Buttons;

        if (!Enabled)
        {
            // Stay in sync so nothing fires the instant the user re-enables.
            _previousButtons = buttons;
            return;
        }

        var pressed = buttons & ~_previousButtons;
        _previousButtons = buttons;

        if (pressed == GamepadButtons.None)
        {
            return;
        }

        _busy = true;
        try
        {
            if (pressed.HasFlag(GamepadButtons.DPadLeft))
            {
                await MediaController.PreviousAsync();
            }
            if (pressed.HasFlag(GamepadButtons.DPadRight))
            {
                await MediaController.NextAsync();
            }
            if (pressed.HasFlag(GamepadButtons.DPadUp))
            {
                VolumeController.VolumeUp();
            }
            if (pressed.HasFlag(GamepadButtons.DPadDown))
            {
                VolumeController.VolumeDown();
            }
            if (pressed.HasFlag(GamepadButtons.A))
            {
                await MediaController.PlayPauseAsync();
            }
        }
        catch
        {
            // Best-effort: a transient failure here (no active media session, etc.)
            // should never take the tray app down.
        }
        finally
        {
            _busy = false;
        }
    }

    public void Dispose()
    {
        _timer.Stop();
        _timer.Dispose();
    }
}
