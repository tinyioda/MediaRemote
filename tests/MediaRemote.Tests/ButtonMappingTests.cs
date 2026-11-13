namespace MediaRemote.Tests;

public class ButtonMappingTests
{
    [Fact]
    public void Map_NoButtonsPressed_ProducesNoActions()
    {
        var actions = ButtonMapping.CreateDefault().Map(RemoteButtons.None);

        Assert.Empty(actions);
    }

    [Theory]
    [InlineData(RemoteButtons.X, RemoteAction.Previous)]
    [InlineData(RemoteButtons.B, RemoteAction.Next)]
    [InlineData(RemoteButtons.A, RemoteAction.PlayPause)]
    [InlineData(RemoteButtons.Y, RemoteAction.Mute)]
    [InlineData(RemoteButtons.LeftShoulder, RemoteAction.VolumeDown)]
    [InlineData(RemoteButtons.RightShoulder, RemoteAction.VolumeUp)]
    public void Map_DefaultMapping_EachButtonProducesItsAction(RemoteButtons button, RemoteAction expected)
    {
        var actions = ButtonMapping.CreateDefault().Map(button);

        var action = Assert.Single(actions);
        Assert.Equal(expected, action);
    }

    [Theory]
    [InlineData(RemoteButtons.Start)]
    [InlineData(RemoteButtons.Back)]
    [InlineData(RemoteButtons.LeftThumb)]
    [InlineData(RemoteButtons.RightThumb)]
    [InlineData(RemoteButtons.DPadUp)]
    [InlineData(RemoteButtons.DPadDown)]
    [InlineData(RemoteButtons.DPadLeft)]
    [InlineData(RemoteButtons.DPadRight)]
    public void Map_UnassignableButton_ProducesNoActions(RemoteButtons button)
    {
        var actions = ButtonMapping.CreateDefault().Map(button);

        Assert.Empty(actions);
    }

    [Fact]
    public void Map_MultipleButtonsAtOnce_ProducesAllOfTheirActions()
    {
        var pressed = RemoteButtons.X | RemoteButtons.RightShoulder;

        var actions = ButtonMapping.CreateDefault().Map(pressed);

        Assert.Equal(new[] { RemoteAction.Previous, RemoteAction.VolumeUp }, actions);
    }

    [Fact]
    public void Map_MappedButtonPlusUnassignableButton_IgnoresTheUnassignableOne()
    {
        var pressed = RemoteButtons.A | RemoteButtons.Start;

        var actions = ButtonMapping.CreateDefault().Map(pressed);

        var action = Assert.Single(actions);
        Assert.Equal(RemoteAction.PlayPause, action);
    }

    [Fact]
    public void FromAssignments_ValidBijection_RoundTripsThroughAssignments()
    {
        var swapped = new Dictionary<RemoteAction, RemoteButtons>(ButtonMapping.CreateDefault().Assignments)
        {
            [RemoteAction.Previous] = RemoteButtons.B,
            [RemoteAction.Next] = RemoteButtons.X,
        };

        var mapping = ButtonMapping.FromAssignments(swapped);

        Assert.Equal(RemoteButtons.B, mapping.GetButton(RemoteAction.Previous));
        Assert.Equal(RemoteButtons.X, mapping.GetButton(RemoteAction.Next));
    }

    [Fact]
    public void FromAssignments_CustomMapping_MapHonorsTheNewAssignment()
    {
        var swapped = new Dictionary<RemoteAction, RemoteButtons>(ButtonMapping.CreateDefault().Assignments)
        {
            [RemoteAction.Previous] = RemoteButtons.B,
            [RemoteAction.Next] = RemoteButtons.X,
        };
        var mapping = ButtonMapping.FromAssignments(swapped);

        // X now triggers Next (not Previous, the default), B now triggers Previous.
        Assert.Equal(new[] { RemoteAction.Next }, mapping.Map(RemoteButtons.X));
        Assert.Equal(new[] { RemoteAction.Previous }, mapping.Map(RemoteButtons.B));
    }

    [Fact]
    public void FromAssignments_MissingAction_Throws()
    {
        var incomplete = new Dictionary<RemoteAction, RemoteButtons>(ButtonMapping.CreateDefault().Assignments);
        incomplete.Remove(RemoteAction.Mute);

        Assert.Throws<ArgumentException>(() => ButtonMapping.FromAssignments(incomplete));
    }

    [Fact]
    public void FromAssignments_DuplicateButton_Throws()
    {
        var duplicated = new Dictionary<RemoteAction, RemoteButtons>(ButtonMapping.CreateDefault().Assignments)
        {
            [RemoteAction.Next] = RemoteButtons.X, // X was already Previous's button
        };

        Assert.Throws<ArgumentException>(() => ButtonMapping.FromAssignments(duplicated));
    }

    [Fact]
    public void FromAssignments_NonAssignableButton_Throws()
    {
        var invalid = new Dictionary<RemoteAction, RemoteButtons>(ButtonMapping.CreateDefault().Assignments)
        {
            [RemoteAction.PlayPause] = RemoteButtons.Start,
        };

        Assert.Throws<ArgumentException>(() => ButtonMapping.FromAssignments(invalid));
    }
}
