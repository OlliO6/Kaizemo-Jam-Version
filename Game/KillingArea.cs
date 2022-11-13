using System.Collections.Generic;
using Additions;
using Godot;

public class KillingArea : Area2D
{
    public override void _Ready()
    {
        Connect("body_entered", this, nameof(OnBodyEntered));
    }

    private void OnBodyEntered(Node body)
    {
        if (body is IKillable killable)
            killable.Kill();
    }
}
