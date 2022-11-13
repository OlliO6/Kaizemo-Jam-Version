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
        Debug.Log(this, "body_entered");
        if (body is IKillable killable)
            killable.Kill();
    }
}
