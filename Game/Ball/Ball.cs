using System;
using System.Collections.Generic;
using Additions;
using BetterInspector;
using Godot;

public partial class Ball : RigidBody2D, IDiveGainer, IHoldAndThrowable, IFalling
{
    [NodeRef] public Area2D pickArea;
    [NodeRef] public CollisionShape2D collider;

    [Export] public Vector2 HoldOffset { get; set; }
    [Export] private float ungrabbableTimeAfterRelease;

    [Export(PropertyHint.Range, "0,1"), StartFoldout("Throwing")] private float holderVelocityTakeover;
    [Export] private Vector2 horizontalThrowVelocity;
    [Export] private Vector2 upwardsThrowVelocity;
    [Export, EndFoldout] private Vector2 downwardsThrowVelocity;

    public Node Holder { get; set; }
    public IDiveGainer bound;

    private Vector2 prevHolderVelocity;
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
                pickArea.Monitoring = false;
                collider.Disabled = true;
                Mode = ModeEnum.Static;
                return;
            }

            prevHolderVelocity = Holder is IFalling falling ? falling.Velocity : Vector2.Zero;
            Holder = null;

            Mode = ModeEnum.Character;
            collider.Disabled = false;

            new TimeAwaiter(this, ungrabbableTimeAfterRelease, onCompleted: () =>
            {
                pickArea.Monitoring = true;

            });
        }
    }

    public Vector2 Velocity
    {
        get => LinearVelocity;
        set => LinearVelocity = value;
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
                Velocity = (upwardsThrowVelocity);
                break;

            case Player.ActionDirection.Down:
                Velocity = (downwardsThrowVelocity);
                break;

            case Player.ActionDirection.Left:
                Velocity = (horizontalThrowVelocity * new Vector2(-1, 1));
                break;

            case Player.ActionDirection.Right:
                Velocity = (horizontalThrowVelocity);
                break;
        }

        Debug.Log(this, prevHolderVelocity);
        Velocity += prevHolderVelocity * holderVelocityTakeover;
    }
}
