using System;
using System.Collections.Generic;
using Additions;
using BetterInspector;
using Godot;
using Shaking;

public partial class Player : KinematicBody2D
{
    const float JumpLenienceTime = 0.1f;

    [NodeRef] public AnimationTree anim;
    [NodeRef] public Particles2D jumpParticles;

    [Export, StartFoldout("Movement")] public float jumpVelocity;
    [Export] public float gravity, jumpingGravity, maxFallingSpeed;
    [Export(PropertyHint.Range, "0,1")] public float jumpCancelStrenght;
    [Export] public float groundedAcceleration, airAcceleration;
    [Export(PropertyHint.Range, "0,1")] public float groundedDamping;
    [Export(PropertyHint.Range, "0,1"), EndFoldout] public float airDamping;


    public Vector2 velocity;
    private bool isJumping, isGrounded;

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
        float horizontalInput = InputManager.GetPlayerHorizontalInput();

        HandleHorizontalMovement();
        HandleVerticalMovement();
        Animate(horizontalInput);

        velocity = MoveAndSlide(velocity, Vector2.Up, maxSlides: isJumping ? 1 : 4);
        isGrounded = IsOnFloor();

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

    private void Animate(float horizontalInput)
    {
        Debug.LogPFrame(this, $"Is grounded: {isGrounded}");
        if (!isGrounded)
        {
            anim.SetParam("State/current", (int)AnimationState.InAir);
            anim.SetParam("FallSpeed/blend_position", velocity.y);
            return;
        }

        if (horizontalInput != 0)
        {
            anim.SetParam("State/current", (int)AnimationState.Run);
            anim.SetParam("RunSpeed/scale", horizontalInput.Abs());
            return;
        }

        anim.SetParam("State/current", (int)AnimationState.Idle);
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


    private enum AnimationState
    {
        Idle,
        Run,
        InAir
    }
}
