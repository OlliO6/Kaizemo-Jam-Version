using System;
using System.Collections.Generic;
using Additions;
using BetterInspector;
using Godot;

public partial class MainMenu : CanvasLayer
{
    [NodeRef] public Button playButton, quitButton;

    [Export(PropertyHint.File, "*.tscn,*.scn")] private string gameScenePath;

    public PackedScene gameScene;

    partial void OnReady()
    {
        gameScene = GD.Load<PackedScene>(gameScenePath);

        playButton.GrabFocus();

        playButton.Connect("pressed", this, nameof(OnPlayPressed));
        quitButton.Connect("pressed", this, nameof(OnQuitPressed));
    }

    private void OnPlayPressed()
    {
        GetTree().ChangeSceneTo(gameScene);
    }

    private void OnQuitPressed()
    {
        GetTree().Quit();
    }
}
