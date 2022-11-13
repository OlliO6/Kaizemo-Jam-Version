using System;
using System.Collections.Generic;
using Additions;
using BetterInspector;
using Godot;

public class Finish : Area2D
{
    public event Action Finished;

    // partial void OnReady()
    public override void _Ready()
    {
        Connect("body_entered", this, nameof(OnBodyEntered));
    }

    private void OnBodyEntered(Node node)
    {
        Finished?.Invoke();
    }
}
