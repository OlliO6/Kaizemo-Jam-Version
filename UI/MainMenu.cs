using System;
using System.Collections.Generic;
using Additions;
using BetterInspector;
using Godot;

public partial class MainMenu : CanvasLayer
{
    [NodeRef] public Button playButton, quitButton, playgroundButton;

    [Export(PropertyHint.File, "*.tscn,*.scn")] private string gameScenePath, playgroundScenePath;

    public PackedScene gameScene;
    public PackedScene playgroundScene;

    partial void OnReady()
    {
        gameScene = GD.Load<PackedScene>(gameScenePath);
        playgroundScene = GD.Load<PackedScene>(playgroundScenePath);

        UISoundPlayer.SkipNext();
        playButton.GrabFocus();

        playButton.Connect("pressed", this, nameof(OnPlayPressed));
        quitButton.Connect("pressed", this, nameof(OnQuitPressed));
        playgroundButton.Connect("pressed", this, nameof(OnPlaygroundPressed));

        UISoundPlayer.RecognizeButtons(playButton, quitButton, playgroundButton);
    }

    private void OnPlaygroundPressed() => SceneManager.LoadScene(playgroundScene);

    private void OnPlayPressed() => SceneManager.LoadScene(gameScene);

    private void OnQuitPressed() => GetTree().Quit();
}
