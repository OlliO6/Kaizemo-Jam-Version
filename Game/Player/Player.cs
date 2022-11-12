using System;
using System.Collections.Generic;
using Additions;
using BetterInspector;
using Godot;
using Shaking;

[Additions.Debugging.DefaultColor(nameof(Colors.LightBlue), nameof(Colors.AliceBlue))]
public partial class Player : KinematicBody2D
{
    const float JumpLenienceTime = 0.1f;

    [NodeRef] public Sprite sprite;
    [NodeRef] public AnimationTree anim;
    [NodeRef] public Particles2D jumpParticles, landParticles;

    [Export, StartFoldout("Movement")] public float jumpVelocity;
    [Export] public float gravity, jumpingGravity, maxFallingSpeed;
    [Export(PropertyHint.Range, "0,1")] public float jumpCancelStrenght;
    [Export] public float groundedAcceleration, airAcceleration;
    [Export(PropertyHint.Range, "0,1")] public float groundedDamping;
    [Export(PropertyHint.Range, "0,1"), EndFoldout] public float airDamping;

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

    partial void OnReady()
    {
        Debug.AddWatcher(this, nameof(velocity));
        Debug.AddWatcher(this, nameof(isGrounded));

        AddChild(groundRememberTimer);
    }

    public override void _EnterTree()
    {
        InputManager.JumpReleased += CancelJump;
    }

    public override void _ExitTree()
    {
        InputManager.JumpReleased -= CancelJump;
    }

    public override void _PhysicsProcess(float delta)
    {
        float horizontalInput = InputManager.GetPlayerHorizontalInput();

        HandleHorizontalMovement();
        HandleVerticalMovement();
        Animate(horizontalInput);

        velocity = MoveAndSlide(velocity, Vector2.Up, maxSlides: isJumping ? 1 : 4);

        if (IsOnFloor() != isGrounded)
        {
            isGrounded = IsOnFloor();

            if (isGrounded) Land();
            else LeaveGround();
        }

        void HandleHorizontalMovement()
        {
            FlipDirection();

            if (isGrounded)
            {
                velocity.x += horizontalInput * groundedAcceleration * delta;
                velocity.x *= Mathf.Pow(1f - groundedDamping, delta * 10f);
                return;
            }

            velocity.x += InputManager.GetPlayerHorizontalInput() * airAcceleration * delta;
            velocity.x *= Mathf.Pow(1f - airDamping, delta * 10f);

            void FlipDirection()
            {
                if (horizontalInput > 0)
                {
                    Scale = new(1, 1);
                    RotationDegrees = 0;
                    return;
                }
                if (horizontalInput < 0)
                {
                    Scale = new(1, -1);
                    RotationDegrees = 180;
                }
            }
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

        if (!InputManager.IsJumpHeld)
            CancelJump();

        jumpParticles.Restart();
    }

    private void CancelJump()
    {
        if (!isJumping) return;

        isJumping = false;
        velocity.y *= 1f - jumpCancelStrenght;
    }


    private enum GroundedAnimationState
    {
        Idle,
        Run
    }
}
