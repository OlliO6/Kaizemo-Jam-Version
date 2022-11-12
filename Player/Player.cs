using System;
using System.Collections.Generic;
using Additions;
using BetterInspector;
using Godot;

public partial class Player : KinematicBody2D
{
    const float JumpLenienceTime = 0.1f;

    [NodeRef] public AnimationPlayer animPlayer;

    [Export, StartFoldout("Movement")] public float jumpVelocity;
    [Export] public float gravity, jumpingGravity, maxFallingSpeed;
    [Export(PropertyHint.Range, "0,1")] public float jumpCancelStrenght;
    [Export] public float groundedAcceleration, airAcceleration;
    [Export(PropertyHint.Range, "0,1")] public float groundedDamping;
    [Export(PropertyHint.Range, "0,1"), EndFoldout] public float airDamping;


    public Vector2 velocity;
    private bool isJumping;

    private Timer groundRememberTimer = new()
    {
        OneShot = true,
        WaitTime = JumpLenienceTime
    };

    partial void OnReady()
    {
        Debug.AddWatcher(this, nameof(velocity));
        Debug.AddWatcher(this, nameof(isJumping));

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
        bool isGrounded = IsOnFloor();

        float horizontalInput = InputManager.GetPlayerHorizontalInput();

        HandleHorizontalMovement();
        HandleVerticalMovement();

        velocity = MoveAndSlide(velocity, Vector2.Up);

        Animate(horizontalInput, isGrounded);

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
            if (InputManager.IsJumpBuffered && groundRememberTimer.TimeLeft != 0)
            {
                Jump();
                return;
            }

            if (isGrounded)
            {
                groundRememberTimer.Start();
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

    private void Animate(float horizontalInput, bool isGrounded)
    {
    }

    private void Jump()
    {
        isJumping = true;
        velocity.y = jumpVelocity;

        if (!InputManager.IsJumpHeld)
            CancelJump();
    }

    private void CancelJump()
    {
        if (!isJumping) return;

        isJumping = false;
        velocity.y *= 1f - jumpCancelStrenght;
    }
}
