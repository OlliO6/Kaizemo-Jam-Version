using System;
using System.Collections.Generic;
using Additions;
using Godot;

public class InputManager : Node
{
    const float JumpBufferTime = 0.15f;

    public static InputManager Instance { get; private set; }

    public static event Action JumpPressed, JumpReleased, PausePressed;
    public static event Action<Player.ActionDirection> DirectionalActionPressed;

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
        PauseMode = PauseModeEnum.Process;
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
                JumpPressed?.Invoke();
                return;
            }
            JumpReleased?.Invoke();
            return;
        }

        if (@event.IsActionPressed("ActionUp"))
        {
            DirectionalActionPressed?.Invoke(Player.ActionDirection.Up);
            return;
        }
        if (@event.IsActionPressed("ActionDown"))
        {
            DirectionalActionPressed?.Invoke(Player.ActionDirection.Down);
            return;
        }
        if (@event.IsActionPressed("ActionLeft"))
        {
            DirectionalActionPressed?.Invoke(Player.ActionDirection.Left);
            return;
        }
        if (@event.IsActionPressed("ActionRight"))
        {
            DirectionalActionPressed?.Invoke(Player.ActionDirection.Right);
            return;
        }

        if (@event.IsActionPressed("Pause"))
        {
            PausePressed?.Invoke();
            return;
        }


        if (@event is InputEventKey key && key.Pressed && key.Scancode == (uint)KeyList.F11)
        {
            ToggleFullscreen();
            return;
        }
    }

    private void ToggleFullscreen()
    {
        OS.WindowFullscreen = !OS.WindowFullscreen;
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
