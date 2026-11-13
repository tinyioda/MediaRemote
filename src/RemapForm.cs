namespace MediaRemote;

/// <summary>
/// Lets the user reassign which physical button triggers each action. Editing is
/// done on a local working copy; nothing is applied until Save is clicked. Picking
/// a button that's already used by another action swaps the two, so the mapping
/// always stays a valid one-to-one assignment.
/// </summary>
internal sealed class RemapForm : Form
{
    private static readonly (RemoteAction Action, string Label)[] ActionRows =
    {
        (RemoteAction.PlayPause, "Play / Pause"),
        (RemoteAction.Previous, "Previous track"),
        (RemoteAction.Next, "Next track"),
        (RemoteAction.Mute, "Mute"),
        (RemoteAction.VolumeDown, "Volume down"),
        (RemoteAction.VolumeUp, "Volume up"),
    };

    private static readonly (RemoteButtons Button, string Label)[] ButtonOptions =
    {
        (RemoteButtons.A, "A"),
        (RemoteButtons.B, "B"),
        (RemoteButtons.X, "X"),
        (RemoteButtons.Y, "Y"),
        (RemoteButtons.LeftShoulder, "Left bumper"),
        (RemoteButtons.RightShoulder, "Right bumper"),
    };

    private readonly Dictionary<RemoteAction, RemoteButtons> _working;
    private readonly Dictionary<RemoteAction, ComboBox> _combos = new();
    private bool _updatingUi;

    public ButtonMapping? Result { get; private set; }

    public RemapForm(ButtonMapping current)
    {
        _working = new Dictionary<RemoteAction, RemoteButtons>(current.Assignments);

        Text = "Remap Controls";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Padding = new Padding(12);

        var grid = new TableLayoutPanel
        {
            ColumnCount = 2,
            RowCount = ActionRows.Length,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
        };

        for (var i = 0; i < ActionRows.Length; i++)
        {
            var (action, label) = ActionRows[i];

            var nameLabel = new Label
            {
                Text = label,
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(3, 8, 16, 3),
            };

            var combo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 130 };
            foreach (var (_, buttonLabel) in ButtonOptions)
            {
                combo.Items.Add(buttonLabel);
            }

            combo.SelectedIndexChanged += (_, _) => OnComboChanged(action, combo);

            grid.Controls.Add(nameLabel, 0, i);
            grid.Controls.Add(combo, 1, i);
            _combos[action] = combo;
        }

        var restoreButton = new Button { Text = "Restore Defaults", AutoSize = true };
        restoreButton.Click += (_, _) => OnRestoreDefaults();

        var cancelButton = new Button { Text = "Cancel", AutoSize = true, DialogResult = DialogResult.Cancel };

        var saveButton = new Button { Text = "Save", AutoSize = true, DialogResult = DialogResult.OK };
        saveButton.Click += (_, _) => OnSave();

        var buttonPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Bottom,
            Margin = new Padding(0, 12, 0, 0),
        };
        buttonPanel.Controls.Add(saveButton);
        buttonPanel.Controls.Add(cancelButton);
        buttonPanel.Controls.Add(restoreButton);

        var outer = new TableLayoutPanel
        {
            ColumnCount = 1,
            RowCount = 2,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
        };
        outer.Controls.Add(grid, 0, 0);
        outer.Controls.Add(buttonPanel, 0, 1);

        Controls.Add(outer);
        AcceptButton = saveButton;
        CancelButton = cancelButton;

        RefreshCombos();
    }

    private void OnComboChanged(RemoteAction action, ComboBox combo)
    {
        if (_updatingUi || combo.SelectedIndex < 0)
        {
            return;
        }

        var newButton = ButtonOptions[combo.SelectedIndex].Button;
        var oldButton = _working[action];
        if (newButton == oldButton)
        {
            return;
        }

        // Whichever action currently owns the newly chosen button takes this
        // action's old button, so the mapping stays a valid one-to-one swap.
        foreach (var other in _working.Keys)
        {
            if (other != action && _working[other] == newButton)
            {
                _working[other] = oldButton;
                break;
            }
        }

        _working[action] = newButton;
        RefreshCombos();
    }

    private void OnRestoreDefaults()
    {
        var defaults = ButtonMapping.CreateDefault();
        foreach (var action in _working.Keys.ToArray())
        {
            _working[action] = defaults.GetButton(action);
        }

        RefreshCombos();
    }

    private void OnSave()
    {
        Result = ButtonMapping.FromAssignments(_working);
    }

    private void RefreshCombos()
    {
        _updatingUi = true;
        foreach (var (action, combo) in _combos)
        {
            var button = _working[action];
            combo.SelectedIndex = Array.FindIndex(ButtonOptions, o => o.Button == button);
        }

        _updatingUi = false;
    }
}
