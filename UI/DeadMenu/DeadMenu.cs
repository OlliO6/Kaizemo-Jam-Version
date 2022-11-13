using System;
using System.Collections.Generic;
using Additions;
using BetterInspector;
using Godot;

public partial class DeadMenu : PopupDialog
{
    [NodeRef] public Button restartButton, menuButton;

    [Export] private PackedScene menuScene;

    partial void OnReady()
    {
        restartButton.Connect("pressed", this, nameof(OnRestartPressed));
        menuButton.Connect("pressed", this, nameof(OnMenuPressed));

        UISoundPlayer.RecognizeButtons(restartButton, menuButton);
    }

    private void OnMenuPressed() => SceneManager.LoadMenu();

    private void OnRestartPressed() => SceneManager.ReloadCurrentSceneOrLevel();

    public void OnPlayerDied()
    {
        GetTree().Paused = true;
        UISoundPlayer.SkipNext();
        PopupCentered();
    }
}
