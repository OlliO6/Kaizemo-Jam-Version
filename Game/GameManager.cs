using System;
using System.Collections.Generic;
using Additions;
using BetterInspector;
using Godot;

public partial class GameManager : Node
{
    [NodeRef] public AudioStreamPlayer musicPlayer;

    [Export] public Godot.Collections.Array<PackedScene> levels = new();
    [Export] private int _currentLevelIdx;

    public Level currentLevel;

    public int CurrentLevelIdx
    {
        get => _currentLevelIdx;
        set
        {
            _currentLevelIdx = value;
            CallDeferred(nameof(SwitchLevel));
        }
    }

    partial void OnReady()
    {
        SwitchLevel();
    }

    private void SwitchLevel()
    {
        if (currentLevel != null)
        {
            RemoveChild(currentLevel);
            currentLevel.QueueFree();
        }

        currentLevel = levels[CurrentLevelIdx].Instance<Level>();
        AddChild(currentLevel);

        musicPlayer.Stream = currentLevel.music;
        currentLevel.Finished += OnLevelFinished;
    }

    private void OnLevelFinished()
    {
        if (CurrentLevelIdx + 1 >= levels.Count)
        {
            Debug.LogU(this, "Finished all levels");
            return;
        }

        CurrentLevelIdx++;
    }
}
