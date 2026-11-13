using System.Runtime.InteropServices;

namespace MediaRemote;

/// <summary>
/// Simulates the hardware volume keys. There's no WinRT session API for master
/// system volume, so this sends the same virtual keys a physical volume button does.
/// </summary>
internal static class VolumeController
{
    private const byte VK_VOLUME_MUTE = 0xAD;
    private const byte VK_VOLUME_DOWN = 0xAE;
    private const byte VK_VOLUME_UP = 0xAF;
    private const uint KEYEVENTF_KEYUP = 0x2;

    [DllImport("user32.dll")]
    private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

    public static void VolumeUp() => Press(VK_VOLUME_UP);

    public static void VolumeDown() => Press(VK_VOLUME_DOWN);

    public static void ToggleMute() => Press(VK_VOLUME_MUTE);

    private static void Press(byte vk)
    {
        keybd_event(vk, 0, 0, UIntPtr.Zero);
        keybd_event(vk, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
    }
}
