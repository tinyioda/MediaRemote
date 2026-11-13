using Windows.Gaming.Input;

namespace MediaRemote;

/// <summary>
/// Polls the first connected Xbox controller and maps a few buttons to media/volume
/// actions. Button mapping, change here if you want different controls:
///   X -> previous track
///   B -> next track
///   A -> play/pause
///   Y -> mute
///   Left bumper  -> volume down
///   Right bumper -> volume up
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
            if (pressed.HasFlag(GamepadButtons.X))
            {
                await MediaController.PreviousAsync();
            }
            if (pressed.HasFlag(GamepadButtons.B))
            {
                await MediaController.NextAsync();
            }
            if (pressed.HasFlag(GamepadButtons.A))
            {
                await MediaController.PlayPauseAsync();
            }
            if (pressed.HasFlag(GamepadButtons.Y))
            {
                VolumeController.ToggleMute();
            }
            if (pressed.HasFlag(GamepadButtons.LeftShoulder))
            {
                VolumeController.VolumeDown();
            }
            if (pressed.HasFlag(GamepadButtons.RightShoulder))
            {
                VolumeController.VolumeUp();
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
