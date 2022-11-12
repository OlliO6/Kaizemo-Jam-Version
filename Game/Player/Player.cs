using System;
using System.Collections.Generic;
using Additions;
using BetterInspector;
using Godot;
using Shaking;

[Additions.Debugging.DefaultColor(nameof(Colors.LightBlue), nameof(Colors.AliceBlue))]
public partial class Player : KinematicBody2D, IDiveGainer
{
    const float JumpLenienceTime = 0.1f;

    [NodeRef] public Sprite sprite;
    [NodeRef] public AnimationTree anim;
    [NodeRef] public Particles2D jumpParticles, landParticles, diveParticles;
    [NodeRef] public RemoteTransform2D heldItemRemote;

    [Export, StartFoldout("Movement")] public float jumpVelocity;
    [Export] public float gravity, jumpingGravity, maxFallingSpeed;
    [Export(PropertyHint.Range, "0,1")] public float jumpCancelStrenght;
    [Export] public float groundedAcceleration, airAcceleration;
    [Export(PropertyHint.Range, "0,1")] public float groundDamping;
    [Export(PropertyHint.Range, "0,1")] public float groundedStopDamping;
    [Export(PropertyHint.Range, "0,1")] public float airDamping;
    [Export] public Vector2 diveUpVelocity;
    [Export, EndFoldout] public Vector2 diveHorizontalVelocity;

    public IHoldAndThrowable heldItem;
    public Vector2 velocity;
    private bool isJumping, isGrounded, _canDive;

    private Timer groundRememberTimer = new()
    {
        OneShot = true,
        WaitTime = JumpLenienceTime
    };

    public bool CanDive
    {
        get => _canDive;
        set
        {
            if (value == _canDive) return;
            _canDive = value;

            sprite.Material.Set("shader_param/apply", value);
        }
    }

    public bool FaceLeft
    {
        get => RotationDegrees is 180 or -180;
        set
        {
            if (value)
            {
                Scale = new(1, -1);
                RotationDegrees = 180;
                return;
            }

            Scale = new(1, 1);
            RotationDegrees = 0;
        }
    }

    partial void OnReady()
    {
        Debug.AddWatcher(this, nameof(velocity));
        Debug.AddWatcher(this, nameof(isGrounded));

        AddChild(groundRememberTimer);
    }

    public override void _EnterTree()
    {
        InputManager.JumpReleased += CancelJump;
        InputManager.DiveInput += OnDiveInput;
        InputManager.ThrowInput += OnThrowInput;
    }

    public override void _ExitTree()
    {
        InputManager.JumpReleased -= CancelJump;
        InputManager.DiveInput -= OnDiveInput;
        InputManager.ThrowInput -= OnThrowInput;
    }

    public override void _PhysicsProcess(float delta)
    {
        float horizontalInput = InputManager.GetPlayerHorizontalInput();

        HandleHorizontalMovement();
        HandleVerticalMovement();
        Animate(horizontalInput);

        ApplyVelocity();

        if (IsOnFloor() != isGrounded)
        {
            isGrounded = IsOnFloor();

            if (isGrounded) Land();
            else LeaveGround();
        }

        void HandleHorizontalMovement()
        {
            if (horizontalInput != 0)
                FaceLeft = horizontalInput < 0;

            if (isGrounded)
            {
                velocity.x += horizontalInput * groundedAcceleration * delta;
                velocity.x *= Mathf.Pow(1f - (horizontalInput == 0 ? groundedStopDamping : groundDamping), delta * 10f);
                return;
            }

            velocity.x += InputManager.GetPlayerHorizontalInput() * airAcceleration * delta;
            velocity.x *= Mathf.Pow(1f - airDamping, delta * 10f);
        }

        void HandleVerticalMovement()
        {
            if (InputManager.IsJumpBuffered && (isGrounded || groundRememberTimer.TimeLeft != 0))
            {
                Jump();
                return;
            }

            if (isGrounded)
            {
                velocity.y = 1;
                isJumping = false;
                return;
            }

            if (velocity.y > 0) isJumping = false;

            velocity.y += (isJumping ? jumpingGravity : gravity) * delta;
            if (velocity.y > maxFallingSpeed)
                velocity.y = maxFallingSpeed;
        }

        void ApplyVelocity()
        {
            Debug.LogPFrame(this, isJumping);
            if (isJumping)
            {
                velocity = MoveAndSlide(velocity, Vector2.Up, maxSlides: 1, infiniteInertia: false);
                return;
            }
            velocity = MoveAndSlide(velocity, Vector2.Up, infiniteInertia: false);
        }
    }

    private void LeaveGround()
    {
        groundRememberTimer.Start();
        anim.SetParam("Land/active", false);
    }

    private void Land()
    {
        CanDive = false;
        landParticles.Restart();
        anim.SetParam("Land/active", true);
    }

    private void Animate(float horizontalInput)
    {
        if (!isGrounded)
        {
            anim.SetParam("Grounded/current", 0);
            anim.SetParam("FallSpeed/blend_position", velocity.y);
            return;
        }

        anim.SetParam("Grounded/current", 1);

        if (horizontalInput != 0)
        {
            anim.SetParam("GroundedState/current", (int)GroundedAnimationState.Run);
            anim.SetParam("RunSpeed/scale", horizontalInput.Abs());
            return;
        }

        anim.SetParam("GroundedState/current", (int)GroundedAnimationState.Idle);
    }

    private void Jump()
    {
        isJumping = true;
        velocity.y = jumpVelocity;

        InputManager.UseJumpBuffer();

        if (!InputManager.IsHoldingJump)
            CancelJump();

        jumpParticles.Restart();
    }

    private void CancelJump()
    {
        if (!isJumping) return;

        isJumping = false;
        velocity.y *= 1f - jumpCancelStrenght;
    }

    private void OnDiveInput(ActionDirection direction)
    {
        if (!CanDive) return;
        CanDive = false;
        Dive(direction);
    }

    private void OnThrowInput(ActionDirection direction)
    {
        if (heldItem == null)
            return;

        heldItemRemote.RemotePath = "";

        heldItem.IsPicked = false;
        heldItem.Throw(direction);
        heldItem = null;
    }

    private void Dive(ActionDirection direction)
    {
        isJumping = false;

        switch (direction)
        {
            case ActionDirection.Up:
                velocity = diveUpVelocity * new Vector2(InputManager.GetPlayerHorizontalInput(), 1);
                break;

            case ActionDirection.Down:
                velocity = diveUpVelocity * new Vector2(InputManager.GetPlayerHorizontalInput(), -1);
                break;

            case ActionDirection.Left:
                FaceLeft = true;
                velocity = diveHorizontalVelocity * new Vector2(-1, 1);
                break;

            case ActionDirection.Right:
                FaceLeft = false;
                velocity = diveHorizontalVelocity;
                break;
        }

        diveParticles.Restart();
    }

    public void GainDive()
    {
        CanDive = true;
    }

    public void PickupUpThrowable(Node2D node)
    {
        if (node is not IHoldAndThrowable item) return;

        heldItemRemote.Position = item.HoldOffset;
        heldItemRemote.RemotePath = heldItemRemote.GetPathTo(node);

        heldItem = item;
        item.IsPicked = true;

        if (item.CanExtendAirTime && !isGrounded && InputManager.IsHoldingJump)
        {
            Jump();
        }
    }

    public enum ActionDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    private enum GroundedAnimationState
    {
        Idle,
        Run
    }
}
