using System.Runtime.InteropServices;

namespace MediaRemote;

/// <summary>
/// Polls the first connected Xbox controller via XInput and dispatches whatever
/// ButtonMapper says newly-pressed buttons map to. XInput (not Windows.Gaming.Input)
/// is used deliberately: WGI's Gamepad/RawGameController only deliver live readings
/// to the foreground window for classic Win32 apps, so a background tray app like
/// this one sees the controller but never gets button state from it. XInput has no
/// such restriction.
/// </summary>
internal sealed class GamepadService : IDisposable
{
    [StructLayout(LayoutKind.Sequential)]
    private struct XInputGamepad
    {
        public ushort wButtons;
        public byte bLeftTrigger;
        public byte bRightTrigger;
        public short sThumbLX;
        public short sThumbLY;
        public short sThumbRX;
        public short sThumbRY;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct XInputState
    {
        public uint dwPacketNumber;
        public XInputGamepad Gamepad;
    }

    [DllImport("xinput1_4.dll")]
    private static extern int XInputGetState(int dwUserIndex, ref XInputState pState);

    private readonly System.Windows.Forms.Timer _timer;
    private RemoteButtons _previousButtons = RemoteButtons.None;
    private int _connectedIndex = -1;
    private bool _busy;

    public bool Enabled { get; set; } = true;

    public bool IsConnected { get; private set; }

    /// <summary>Raised on the UI thread whenever the controller connects or disconnects.</summary>
    public event EventHandler<bool>? ConnectionChanged;

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

        try
        {
            var connected = TryGetConnectedState(out var buttons);
            if (connected != IsConnected)
            {
                IsConnected = connected;
                ConnectionChanged?.Invoke(this, connected);
            }

            if (!connected)
            {
                // Clear so a reconnect starts clean and can't fire a phantom
                // "newly pressed" action from stale state on the first tick back.
                _previousButtons = RemoteButtons.None;
                return;
            }

            if (!Enabled)
            {
                // Stay in sync so nothing fires the instant the user re-enables.
                _previousButtons = buttons;
                return;
            }

            var pressed = buttons & ~_previousButtons;
            _previousButtons = buttons;

            var actions = ButtonMapper.Map(pressed);
            if (actions.Count == 0)
            {
                return;
            }

            _busy = true;
            foreach (var action in actions)
            {
                await DispatchAsync(action);
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

    private static Task DispatchAsync(RemoteAction action) => action switch
    {
        RemoteAction.Previous => MediaController.PreviousAsync(),
        RemoteAction.Next => MediaController.NextAsync(),
        RemoteAction.PlayPause => MediaController.PlayPauseAsync(),
        RemoteAction.Mute => RunSync(VolumeController.ToggleMute),
        RemoteAction.VolumeDown => RunSync(VolumeController.VolumeDown),
        RemoteAction.VolumeUp => RunSync(VolumeController.VolumeUp),
        _ => Task.CompletedTask,
    };

    private static Task RunSync(Action action)
    {
        action();
        return Task.CompletedTask;
    }

    private bool TryGetConnectedState(out RemoteButtons buttons)
    {
        // Stick with whichever slot was last connected to avoid flip-flopping
        // between slots if more than one controller is ever plugged in.
        if (_connectedIndex >= 0)
        {
            var state = default(XInputState);
            if (XInputGetState(_connectedIndex, ref state) == 0)
            {
                buttons = (RemoteButtons)state.Gamepad.wButtons;
                return true;
            }

            _connectedIndex = -1;
        }

        for (var i = 0; i < 4; i++)
        {
            var state = default(XInputState);
            if (XInputGetState(i, ref state) == 0)
            {
                _connectedIndex = i;
                buttons = (RemoteButtons)state.Gamepad.wButtons;
                return true;
            }
        }

        buttons = RemoteButtons.None;
        return false;
    }

    public void Dispose()
    {
        _timer.Stop();
        _timer.Dispose();
    }
}
