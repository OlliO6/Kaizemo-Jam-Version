using System;
using System.Collections.Generic;
using Additions;
using Godot;

public class InputManager : Node
{
    const float JumpBufferTime = 0.15f;

    public static InputManager Instance { get; private set; }

    public static event Action JumpPressed;
    public static event Action JumpReleased;
    public static event Action<Player.ActionDirection> ThrowInput;
    public static event Action<Player.ActionDirection> DiveInput;

    public static bool IsJumpBuffered => Instance.jumpTimer.TimeLeft != 0 && !Instance.jumpTimer.IsStopped();
    public static bool IsHoldingJump => Input.IsActionPressed("Jump");

    private Timer jumpTimer = new()
    {
        OneShot = true,
        WaitTime = JumpBufferTime
    };

    public override void _EnterTree() => Instance = this;

    public override void _ExitTree() => Instance = null;

    public override void _Ready()
    {
        JumpPressed += StartJumpBuffer;
        AddChild(jumpTimer);
    }

    public static float GetPlayerHorizontalInput()
    {
        return Input.GetAxis("MoveLeft", "MoveRight");
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsEcho())
        {
            return;
        }

        if (@event.IsAction("Jump"))
        {
            if (@event.IsPressed())
            {
                JumpPressed();
                return;
            }
            JumpReleased();
            return;
        }

        if (@event.IsActionPressed("ActionUp"))
        {
            (Input.IsActionPressed("Throw") ? ThrowInput : DiveInput)
                    ?.Invoke(Player.ActionDirection.Up);
            return;
        }
        if (@event.IsActionPressed("ActionDown"))
        {
            (Input.IsActionPressed("Throw") ? ThrowInput : DiveInput)
                    ?.Invoke(Player.ActionDirection.Down);
            return;
        }
        if (@event.IsActionPressed("ActionLeft"))
        {
            (Input.IsActionPressed("Throw") ? ThrowInput : DiveInput)
                    ?.Invoke(Player.ActionDirection.Left);
            return;
        }
        if (@event.IsActionPressed("ActionRight"))
        {
            (Input.IsActionPressed("Throw") ? ThrowInput : DiveInput)
                    ?.Invoke(Player.ActionDirection.Right);
            return;
        }
    }

    private void StartJumpBuffer()
    {
        jumpTimer.Start();
    }

    public static void UseJumpBuffer()
    {
        Instance.jumpTimer.Stop();
    }
}
