namespace MediaRemote;

internal sealed class TrayApplicationContext : ApplicationContext
{
    private readonly GamepadService _gamepadService = new();
    private readonly NotifyIcon _notifyIcon;
    private readonly ToolStripMenuItem _statusItem;
    private readonly ToolStripMenuItem _toggleItem;
    private readonly Icon _icon = RemoteIcon.Create();

    public TrayApplicationContext()
    {
        _statusItem = new ToolStripMenuItem(string.Empty) { Enabled = false };
        _toggleItem = new ToolStripMenuItem(string.Empty, null, OnToggleClicked);
        var remapItem = new ToolStripMenuItem("Remap...", null, OnRemapClicked);
        var closeItem = new ToolStripMenuItem("Close", null, OnCloseClicked);

        var menu = new ContextMenuStrip();
        menu.Items.Add(_statusItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(_toggleItem);
        menu.Items.Add(remapItem);
        menu.Items.Add(closeItem);

        _notifyIcon = new NotifyIcon
        {
            Icon = _icon,
            Text = "MediaRemote",
            ContextMenuStrip = menu,
            Visible = true,
        };

        _gamepadService.ConnectionChanged += (_, connected) => UpdateStatusLabel(connected);

        UpdateStatusLabel(_gamepadService.IsConnected);
        UpdateToggleLabel();
    }

    private void OnToggleClicked(object? sender, EventArgs e)
    {
        _gamepadService.Enabled = !_gamepadService.Enabled;
        UpdateToggleLabel();
    }

    private void UpdateStatusLabel(bool connected)
    {
        _statusItem.Text = connected ? "Controller: Connected" : "Controller: Disconnected";
    }

    private void UpdateToggleLabel()
    {
        _toggleItem.Text = _gamepadService.Enabled ? "Disable" : "Enable";
        _notifyIcon.Text = _gamepadService.Enabled ? "MediaRemote (active)" : "MediaRemote (paused)";
    }

    private void OnRemapClicked(object? sender, EventArgs e)
    {
        using var form = new RemapForm(_gamepadService.Mapping);
        if (form.ShowDialog() == DialogResult.OK && form.Result is not null)
        {
            _gamepadService.UpdateMapping(form.Result);
        }
    }

    private void OnCloseClicked(object? sender, EventArgs e)
    {
        _notifyIcon.Visible = false;
        _gamepadService.Dispose();
        _icon.Dispose();
        ExitThread();
    }
}
