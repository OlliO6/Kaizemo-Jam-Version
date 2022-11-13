using System;
using System.Collections.Generic;
using Additions;
using BetterInspector;
using Godot;
using Shaking;

[Additions.Debugging.DefaultColor(nameof(Colors.LightBlue), nameof(Colors.AliceBlue))]
public partial class Player : KinematicBody2D, IDiveGainer, IFalling, IKillable
{
    const float JumpLenienceTime = 0.1f;

    [NodeRef] public Sprite sprite;
    [NodeRef] public AnimationTree anim;
    [NodeRef] public Particles2D jumpParticles, landParticles, diveParticles;
    [NodeRef] public RemoteTransform2D heldItemRemote;
    [NodeRef] public AudioStreamPlayer audioPlayer;
    [NodeRef] public DeadMenu deadMenu;

    [Export, StartFoldout("Movement")] public float jumpVelocity;
    [Export] public float gravity, jumpingGravity, maxFallingSpeed;
    [Export(PropertyHint.Range, "0,1")] public float jumpCancelStrenght;
    [Export] public float groundedAcceleration, airAcceleration;
    [Export(PropertyHint.Range, "0,1")] public float groundDamping;
    [Export(PropertyHint.Range, "0,1")] public float groundedStopDamping;
    [Export(PropertyHint.Range, "0,1")] public float airDamping;
    [Export] public Vector2 diveUpVelocity;
    [Export, EndFoldout] public Vector2 diveHorizontalVelocity;

    [Export, InFoldout("Sounds")] private AudioStream jumpSound, diveSound, gainDiveSound, die;

    [Export] private float dieMenuPopDelay;

    public IHoldAndThrowable heldItem;
    private Vector2 velocity;
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

    public Vector2 Velocity { get => velocity; set => velocity = value; }

    partial void OnReady()
    {
        Debug.AddWatcher(this, nameof(Velocity));
        Debug.AddWatcher(this, nameof(isGrounded));

        AddChild(groundRememberTimer);
        anim.SetParam("Dead/current", 0);
    }

    public override void _EnterTree()
    {
        InputManager.JumpReleased += CancelJump;
        InputManager.DirectionalActionPressed += OnActionInput;
    }

    public override void _ExitTree()
    {
        InputManager.JumpReleased -= CancelJump;
        InputManager.DirectionalActionPressed -= OnActionInput;
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
                Jump(jumpVelocity);
                return;
            }

            if (isGrounded)
            {
                velocity.y = 1;
                isJumping = false;
                return;
            }

            if (Velocity.y > 0) isJumping = false;

            velocity.y += (isJumping ? jumpingGravity : gravity) * delta;
            if (Velocity.y > maxFallingSpeed)
                velocity.y = maxFallingSpeed;
        }

        void ApplyVelocity()
        {
            Debug.LogPFrame(this, isJumping);
            if (isJumping)
            {
                Velocity = MoveAndSlide(Velocity, Vector2.Up, maxSlides: 1, infiniteInertia: false);
                return;
            }
            Velocity = MoveAndSlide(Velocity, Vector2.Up, infiniteInertia: false);
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
            anim.SetParam("FallSpeed/blend_position", Velocity.y);
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

    private void Jump(float jumpVelocity)
    {
        isJumping = true;
        velocity.y = jumpVelocity;

        InputManager.UseJumpBuffer();

        if (!InputManager.IsHoldingJump)
            CancelJump();

        PlaySound(jumpSound);
        jumpParticles.Restart();
    }

    private void PlaySound(AudioStream sound)
    {
        audioPlayer.Stream = sound;
        audioPlayer.Play();
    }

    private void CancelJump()
    {
        if (!isJumping) return;

        isJumping = false;
        velocity.y *= 1f - jumpCancelStrenght;
    }

    private void OnActionInput(ActionDirection direction)
    {
        if (heldItem == null)
        {
            Dive(direction);
            return;
        }

        CallDeferred(nameof(ThrowItem), direction);
    }

    private void ThrowItem(ActionDirection direction)
    {
        heldItemRemote.RemotePath = "";
        heldItem.IsPicked = false;
        heldItem.Throw(direction);
        heldItem = null;
    }

    private void Dive(ActionDirection direction)
    {
        if (!CanDive) return;

        CanDive = false;
        isJumping = false;

        switch (direction)
        {
            case ActionDirection.Up:
                Velocity = diveUpVelocity * new Vector2(InputManager.GetPlayerHorizontalInput(), 1);
                break;

            case ActionDirection.Down:
                Velocity = diveUpVelocity * new Vector2(InputManager.GetPlayerHorizontalInput(), -1);
                break;

            case ActionDirection.Left:
                FaceLeft = true;
                Velocity = diveHorizontalVelocity * new Vector2(-1, 1);
                break;

            case ActionDirection.Right:
                FaceLeft = false;
                Velocity = diveHorizontalVelocity;
                break;
        }

        PlaySound(diveSound);
        diveParticles.Restart();
    }

    public void GainDive()
    {
        CanDive = true;
        PlaySound(gainDiveSound);
    }

    public void PickupUpThrowable(Node2D node)
    {
        if (node is not IHoldAndThrowable item) return;

        heldItemRemote.Position = item.HoldOffset;
        heldItemRemote.RemotePath = heldItemRemote.GetPathTo(node);

        heldItem = item;
        item.IsPicked = true;
        item.Holder = this;

        if (item.CanExtendAirTime && !isGrounded && InputManager.IsHoldingJump)
        {
            Jump(item.ExtendAirVelocity);
        }
    }

    public void Kill()
    {
        SetProcess(false);
        SetPhysicsProcess(false);
        PauseMode = PauseModeEnum.Process;

        anim.SetParam("Dead/current", 1);

        PlaySound(die);

        new TimeAwaiter(this, dieMenuPopDelay,
            onCompleted: deadMenu.OnPlayerDied);

        Debug.LogU(this, "Died");
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
