using System;
using System.Collections.Generic;
using Additions;
using BetterInspector;
using Godot;

public partial class DiveSpender : Area2D
{
    [NodeRef] public Sprite sprite;

    partial void OnReady()
    {
        Connect("body_entered", this, nameof(OnBodyEntered));
    }

    private void OnBodyEntered(Node node)
    {
        if (node is not Player player)
            return;

        player.CanDive = true;
    }
}
