using System;
using System.Collections.Generic;
using Additions;
using BetterInspector;
using Godot;

[Tool]
public partial class Level : Node2D
{
    public event Action Finished;

    [NodeRef] public Finish finish;
    [NodeRef] public Player player;

    [Export] public AudioStream music;

    private float maxYPos;

    [Export]
    public float MaxYPos
    {
        get => maxYPos;
        set
        {
            maxYPos = value;

            if (!Engine.EditorHint) return;
            CallDeferred(nameof(UpdateMaxYPos));
        }
    }

    private void UpdateMaxYPos()
    {
        GetNode(_player).Set("maxYPos", MaxYPos);
        GetNode<Camera2D>(_player + "/Camera").Set("limit_bottom", (int)MaxYPos);
    }

    partial void OnReady() => finish.Finished += Finish;

    private void Finish()
    {
        Debug.LogU(this, "Finished");
        Finished?.Invoke();
    }
}
