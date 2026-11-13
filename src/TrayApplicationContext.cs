namespace MediaRemote;

internal sealed class TrayApplicationContext : ApplicationContext
{
    private readonly GamepadService _gamepadService = new();
    private readonly NotifyIcon _notifyIcon;
    private readonly ToolStripMenuItem _toggleItem;

    public TrayApplicationContext()
    {
        _toggleItem = new ToolStripMenuItem(string.Empty, null, OnToggleClicked);
        var closeItem = new ToolStripMenuItem("Close", null, OnCloseClicked);

        var menu = new ContextMenuStrip();
        menu.Items.Add(_toggleItem);
        menu.Items.Add(closeItem);

        _notifyIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "MediaRemote",
            ContextMenuStrip = menu,
            Visible = true,
        };

        UpdateToggleLabel();
    }

    private void OnToggleClicked(object? sender, EventArgs e)
    {
        _gamepadService.Enabled = !_gamepadService.Enabled;
        UpdateToggleLabel();
    }

    private void UpdateToggleLabel()
    {
        _toggleItem.Text = _gamepadService.Enabled ? "Disable" : "Enable";
        _notifyIcon.Text = _gamepadService.Enabled ? "MediaRemote (active)" : "MediaRemote (paused)";
    }

    private void OnCloseClicked(object? sender, EventArgs e)
    {
        _notifyIcon.Visible = false;
        _gamepadService.Dispose();
        ExitThread();
    }
}
