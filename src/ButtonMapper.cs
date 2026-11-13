namespace MediaRemote;

/// <summary>
/// Pure button -> action mapping, kept separate from GamepadService so it can be
/// unit tested without XInput/WinForms/WinRT involved.
///
///   X -> previous track
///   B -> next track
///   A -> play/pause
///   Y -> mute
///   Left bumper  -> volume down
///   Right bumper -> volume up
///
/// Every other button (D-pad, Start/Back, thumbstick clicks) is intentionally
/// unmapped and produces no action.
/// </summary>
public static class ButtonMapper
{
    public static IReadOnlyList<RemoteAction> Map(RemoteButtons pressed)
    {
        var actions = new List<RemoteAction>();

        if (pressed.HasFlag(RemoteButtons.X))
        {
            actions.Add(RemoteAction.Previous);
        }
        if (pressed.HasFlag(RemoteButtons.B))
        {
            actions.Add(RemoteAction.Next);
        }
        if (pressed.HasFlag(RemoteButtons.A))
        {
            actions.Add(RemoteAction.PlayPause);
        }
        if (pressed.HasFlag(RemoteButtons.Y))
        {
            actions.Add(RemoteAction.Mute);
        }
        if (pressed.HasFlag(RemoteButtons.LeftShoulder))
        {
            actions.Add(RemoteAction.VolumeDown);
        }
        if (pressed.HasFlag(RemoteButtons.RightShoulder))
        {
            actions.Add(RemoteAction.VolumeUp);
        }

        return actions;
    }
}
