using System;
using System.Collections.Generic;
using Additions;
using BetterInspector;
using Godot;

public partial class Ball : RigidBody2D, IDiveGainer, IHoldAndThrowable
{
    [NodeRef] public Area2D pickArea;
    [NodeRef] public CollisionPolygon2D collider;

    [Export] public Vector2 HoldOffset { get; set; }
    [Export, StartFoldout("Throwing")] private Vector2 horizontalThrowVelocity;
    [Export] private Vector2 upwardsThrowVelocity;
    [Export, EndFoldout] private Vector2 downwardsThrowVelocity;

    public Node2D holder;
    public IDiveGainer bound;

    private bool _picked;

    public bool IsPicked
    {
        get => _picked;
        set
        {
            if (_picked == value)
                return;

            _picked = value;

            if (value)
            {
                collider.Disabled = true;
                Mode = ModeEnum.Static;
                return;
            }
            collider.Disabled = false;
            Mode = ModeEnum.Character;
        }
    }

    public void GainDive() => bound?.GainDive();

    public override void _PhysicsProcess(float delta)
    {
        if (IsPicked)
            LinearVelocity = Vector2.Zero;
    }

    partial void OnReady()
    {
        pickArea.Connect("body_entered", this, nameof(OnBodyEnteredPickArea));
    }

    private void OnBodyEnteredPickArea(Node body)
    {
        if (IsPicked || body is not Player player) return;

        bound = player;
        player.CallDeferred(nameof(Player.PickupUpThrowable), this);
    }

    public void Throw(Player.ActionDirection direction)
    {
        switch (direction)
        {
            case Player.ActionDirection.Up:
                ApplyCentralImpulse(upwardsThrowVelocity);
                break;

            case Player.ActionDirection.Down:
                ApplyCentralImpulse(downwardsThrowVelocity);
                break;

            case Player.ActionDirection.Left:
                ApplyCentralImpulse(horizontalThrowVelocity * new Vector2(-1, 1));
                break;

            case Player.ActionDirection.Right:
                ApplyCentralImpulse(horizontalThrowVelocity);
                break;
        }
    }
}
