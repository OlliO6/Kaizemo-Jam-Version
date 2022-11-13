using System;
using System.Collections.Generic;
using Additions;
using BetterInspector;
using Godot;

public partial class Level : Node2D
{
    public event Action Finished;

    [NodeRef] public Finish finish;
    [NodeRef] public Player player;

    [Export] public AudioStream music;

    partial void OnReady() => finish.Finished += Finish;

    private void Finish()
    {
        Debug.LogU(this, "Finished");
        Finished?.Invoke();
    }
}
