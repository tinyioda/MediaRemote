namespace MediaRemote;

/// <summary>
/// An immutable, validated action -> button assignment. Replaces the old
/// hardcoded ButtonMapper: the mapping is now something the user can change at
/// runtime via the Remap window, instead of being fixed at compile time.
/// </summary>
public sealed class ButtonMapping
{
    /// <summary>The only physical buttons that can be assigned to an action.</summary>
    public static readonly IReadOnlyList<RemoteButtons> AssignableButtons = new[]
    {
        RemoteButtons.A,
        RemoteButtons.B,
        RemoteButtons.X,
        RemoteButtons.Y,
        RemoteButtons.LeftShoulder,
        RemoteButtons.RightShoulder,
    };

    private readonly IReadOnlyDictionary<RemoteAction, RemoteButtons> _assignments;

    private ButtonMapping(IReadOnlyDictionary<RemoteAction, RemoteButtons> assignments)
    {
        _assignments = assignments;
    }

    public IReadOnlyDictionary<RemoteAction, RemoteButtons> Assignments => _assignments;

    public RemoteButtons GetButton(RemoteAction action) => _assignments[action];

    public static ButtonMapping CreateDefault() => new(new Dictionary<RemoteAction, RemoteButtons>
    {
        [RemoteAction.Previous] = RemoteButtons.X,
        [RemoteAction.Next] = RemoteButtons.B,
        [RemoteAction.PlayPause] = RemoteButtons.A,
        [RemoteAction.Mute] = RemoteButtons.Y,
        [RemoteAction.VolumeDown] = RemoteButtons.LeftShoulder,
        [RemoteAction.VolumeUp] = RemoteButtons.RightShoulder,
    });

    /// <summary>
    /// Validates that every action has exactly one assignable button and no two
    /// actions share a button, then builds an immutable mapping from it. Throws
    /// <see cref="ArgumentException"/> if the assignments aren't a valid bijection.
    /// </summary>
    public static ButtonMapping FromAssignments(IReadOnlyDictionary<RemoteAction, RemoteButtons> assignments)
    {
        var allActions = Enum.GetValues<RemoteAction>();
        if (assignments.Count != allActions.Length)
        {
            throw new ArgumentException("Mapping must assign every action exactly once.", nameof(assignments));
        }

        var seenButtons = new HashSet<RemoteButtons>();
        foreach (var action in allActions)
        {
            if (!assignments.TryGetValue(action, out var button))
            {
                throw new ArgumentException($"Mapping is missing an assignment for {action}.", nameof(assignments));
            }

            if (!AssignableButtons.Contains(button))
            {
                throw new ArgumentException($"{button} is not an assignable button.", nameof(assignments));
            }

            if (!seenButtons.Add(button))
            {
                throw new ArgumentException($"{button} is assigned to more than one action.", nameof(assignments));
            }
        }

        return new ButtonMapping(new Dictionary<RemoteAction, RemoteButtons>(assignments));
    }

    /// <summary>Returns every action whose assigned button is set in <paramref name="pressed"/>.</summary>
    public IReadOnlyList<RemoteAction> Map(RemoteButtons pressed)
    {
        var actions = new List<RemoteAction>();
        foreach (var action in Enum.GetValues<RemoteAction>())
        {
            if (pressed.HasFlag(_assignments[action]))
            {
                actions.Add(action);
            }
        }

        return actions;
    }
}
