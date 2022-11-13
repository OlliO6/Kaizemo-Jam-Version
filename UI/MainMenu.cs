using System;
using System.Collections.Generic;
using Additions;
using BetterInspector;
using Godot;

public partial class MainMenu : CanvasLayer
{
    [NodeRef] public Button playButton;

    [Export] private PackedScene gameScene;

    partial void OnReady()
    {
        playButton.Connect("pressed", this, nameof(OnPlayPressed));
    }

    private void OnPlayPressed()
    {
        GetTree().ChangeSceneTo(gameScene);
    }
}
