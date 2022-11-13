using System;
using System.Collections.Generic;
using Additions;
using Godot;

public class UISoundPlayer : AudioStreamPlayer
{
    public static UISoundPlayer Instance { get; private set; }

    public AudioStream blipSound = new AudioStreamRandomPitch()
    {
        RandomPitch = 1.06f,
        AudioStream = GD.Load<AudioStream>("res://Sounds/Blip.wav")
    };

    public AudioStream selectSound = new AudioStreamRandomPitch()
    {
        RandomPitch = 1.06f,
        AudioStream = GD.Load<AudioStream>("res://Sounds/Select.wav")
    };

    public AudioStreamPlayer selectPlayer = new();

    private bool skipNext;


    public override void _EnterTree() => Instance = this;
    public override void _ExitTree() => Instance = null;

    public override void _Ready()
    {
        AddChild(selectPlayer);
        selectPlayer.Stream = selectSound;

        PauseMode = PauseModeEnum.Process;
        Stream = blipSound;
        GetTree().Root.Connect("gui_focus_changed", this, nameof(OnFocusChanged));
    }

    public static void SkipNext() => Instance.skipNext = true;

    public static void Select() => Instance.selectPlayer.Play();

    public static void RecognizeButtons(params BaseButton[] buttons)
    {
        foreach (BaseButton button in buttons)
        {
            if (button.ToggleMode)
            {
                button.Connect("toggled", Instance, nameof(OnButtonToggled));
                continue;
            }
            button.Connect("pressed", Instance, nameof(OnButtonPressed));
        }
    }

    private static void OnButtonPressed() => Select();
    private static void OnButtonToggled(bool _) => Select();

    private void OnFocusChanged(Node to)
    {
        if (skipNext)
        {
            skipNext = false;
            return;
        }

        Play();
    }
}
