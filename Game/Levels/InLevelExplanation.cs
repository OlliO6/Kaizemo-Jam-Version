using System;
using System.Collections.Generic;
using Additions;
using BetterInspector;
using Godot;

public partial class InLevelExplanation : Area2D
{
    [NodeRef(foldout = "")] private CanvasItem toShow;

    partial void OnReady()
    {
        toShow.Hide();
        Connect("body_entered", this, nameof(OnBodyEntered));
        Connect("body_exited", this, nameof(OnBodyExited));
    }

    private void OnBodyExited(Node _)
    {
        toShow.Hide();
    }

    private void OnBodyEntered(Node _)
    {
        toShow.Show();
    }
}
