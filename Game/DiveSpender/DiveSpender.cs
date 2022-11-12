using System;
using System.Collections.Generic;
using Additions;
using BetterInspector;
using Godot;

public partial class DiveSpender : Area2D
{
    [NodeRef] public Sprite sprite;

    [Export] public float deaciveTime;
    [Export] private Color deaciveColor;

    public bool IsActive => IsConnected("body_entered", this, nameof(OnBodyEntered));

    partial void OnReady()
    {
        Connect("body_entered", this, nameof(OnBodyEntered));
    }

    private void OnBodyEntered(Node node)
    {
        if (node is not IDiveGainer diveGainer)
            return;

        diveGainer.GainDive();

        Deactivate();

        new TimeAwaiter(this, deaciveTime,
            onCompleted: Activate);
    }

    public void Deactivate()
    {
        if (!IsActive) return;

        sprite.Modulate = deaciveColor;
        Disconnect("body_entered", this, nameof(OnBodyEntered));
    }

    public void Activate()
    {
        if (IsActive) return;

        sprite.Modulate = Colors.White;
        Connect("body_entered", this, nameof(OnBodyEntered));
    }
}
