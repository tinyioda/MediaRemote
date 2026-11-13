using System.Runtime.InteropServices;

namespace MediaRemote;

/// <summary>
/// Polls the first connected Xbox controller via XInput and maps a few buttons to
/// media/volume actions. XInput (not Windows.Gaming.Input) is used deliberately:
/// WGI's Gamepad/RawGameController only deliver live readings to the foreground
/// window for classic Win32 apps, so a background tray app like this one sees the
/// controller but never gets button state from it. XInput has no such restriction.
///
/// Button mapping, change here if you want different controls:
///   X -> previous track
///   B -> next track
///   A -> play/pause
///   Y -> mute
///   Left bumper  -> volume down
///   Right bumper -> volume up
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

    [Flags]
    private enum Buttons : ushort
    {
        None = 0,
        DPadUp = 0x0001,
        DPadDown = 0x0002,
        DPadLeft = 0x0004,
        DPadRight = 0x0008,
        Start = 0x0010,
        Back = 0x0020,
        LeftThumb = 0x0040,
        RightThumb = 0x0080,
        LeftShoulder = 0x0100,
        RightShoulder = 0x0200,
        A = 0x1000,
        B = 0x2000,
        X = 0x4000,
        Y = 0x8000,
    }

    [DllImport("xinput1_4.dll")]
    private static extern int XInputGetState(int dwUserIndex, ref XInputState pState);

    private readonly System.Windows.Forms.Timer _timer;
    private Buttons _previousButtons = Buttons.None;
    private int _connectedIndex = -1;
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

        try
        {
            if (!TryGetConnectedState(out var buttons))
            {
                _previousButtons = Buttons.None;
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

            if (pressed == Buttons.None)
            {
                return;
            }

            _busy = true;
            if (pressed.HasFlag(Buttons.X))
            {
                await MediaController.PreviousAsync();
            }
            if (pressed.HasFlag(Buttons.B))
            {
                await MediaController.NextAsync();
            }
            if (pressed.HasFlag(Buttons.A))
            {
                await MediaController.PlayPauseAsync();
            }
            if (pressed.HasFlag(Buttons.Y))
            {
                VolumeController.ToggleMute();
            }
            if (pressed.HasFlag(Buttons.LeftShoulder))
            {
                VolumeController.VolumeDown();
            }
            if (pressed.HasFlag(Buttons.RightShoulder))
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

    private bool TryGetConnectedState(out Buttons buttons)
    {
        // Stick with whichever slot was last connected to avoid flip-flopping
        // between slots if more than one controller is ever plugged in.
        if (_connectedIndex >= 0)
        {
            var state = default(XInputState);
            if (XInputGetState(_connectedIndex, ref state) == 0)
            {
                buttons = (Buttons)state.Gamepad.wButtons;
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
                buttons = (Buttons)state.Gamepad.wButtons;
                return true;
            }
        }

        buttons = Buttons.None;
        return false;
    }

    public void Dispose()
    {
        _timer.Stop();
        _timer.Dispose();
    }
}
