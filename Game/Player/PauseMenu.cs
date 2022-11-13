using System;
using System.Collections.Generic;
using Additions;
using BetterInspector;
using Godot;

public partial class PauseMenu : PopupDialog
{
    [NodeRef] public Button resumeButton, restartButton, menuButton;

    [Export] private PackedScene menuScene;

    public override void _EnterTree()
    {
        InputManager.PausePressed += TogglePaused;
    }

    public override void _ExitTree()
    {
        InputManager.PausePressed -= TogglePaused;
    }

    partial void OnReady()
    {
        resumeButton.Connect("pressed", this, nameof(Resume));
        restartButton.Connect("pressed", this, nameof(OnRestartPressed));
        menuButton.Connect("pressed", this, nameof(OnMenuPressed));
    }

    private void OnMenuPressed()
    {
        Resume();
        GetTree().ChangeSceneTo(menuScene);
    }

    private void OnRestartPressed()
    {
        Resume();
        GetTree().ReloadCurrentScene();
    }

    private void TogglePaused()
    {
        if (Visible)
        {
            Resume();
            return;
        }
        Pause();
    }

    private void Resume()
    {
        if (!IsInsideTree()) return;

        GetTree().Paused = false;
        Hide();
    }

    private void Pause()
    {
        if (GetTree().Paused) return;

        GetTree().Paused = true;
        PopupCenteredRatio();
    }
}
