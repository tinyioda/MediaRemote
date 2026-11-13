namespace MediaRemote.Tests;

public class ButtonMapperTests
{
    [Fact]
    public void NoButtonsPressed_ProducesNoActions()
    {
        var actions = ButtonMapper.Map(RemoteButtons.None);

        Assert.Empty(actions);
    }

    [Theory]
    [InlineData(RemoteButtons.X, RemoteAction.Previous)]
    [InlineData(RemoteButtons.B, RemoteAction.Next)]
    [InlineData(RemoteButtons.A, RemoteAction.PlayPause)]
    [InlineData(RemoteButtons.Y, RemoteAction.Mute)]
    [InlineData(RemoteButtons.LeftShoulder, RemoteAction.VolumeDown)]
    [InlineData(RemoteButtons.RightShoulder, RemoteAction.VolumeUp)]
    public void MappedButton_ProducesExactlyItsAction(RemoteButtons button, RemoteAction expected)
    {
        var actions = ButtonMapper.Map(button);

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
    public void UnmappedButton_ProducesNoActions(RemoteButtons button)
    {
        var actions = ButtonMapper.Map(button);

        Assert.Empty(actions);
    }

    [Fact]
    public void MultipleButtonsAtOnce_ProducesAllOfTheirActions()
    {
        var pressed = RemoteButtons.X | RemoteButtons.RightShoulder;

        var actions = ButtonMapper.Map(pressed);

        Assert.Equal(new[] { RemoteAction.Previous, RemoteAction.VolumeUp }, actions);
    }

    [Fact]
    public void MappedButtonPlusUnmappedButton_IgnoresTheUnmappedOne()
    {
        var pressed = RemoteButtons.A | RemoteButtons.Start;

        var actions = ButtonMapper.Map(pressed);

        var action = Assert.Single(actions);
        Assert.Equal(RemoteAction.PlayPause, action);
    }
}
