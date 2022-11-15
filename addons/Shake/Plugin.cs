#if TOOLS
namespace Shaking;

using System;
using Godot;

[Tool]
public class Plugin : EditorPlugin
{
    public override void _EnterTree()
    {
        AddCustomType(nameof(CamShakeInducer), nameof(Node), GD.Load<CSharpScript>("res://addons/Shake/PlayerCamShakeInducer.cs"), null);
        AddCustomType(nameof(Shaker), nameof(Node2D), GD.Load<CSharpScript>("res://addons/Shake/Shaker.cs"), null);
    }

    public override void _ExitTree()
    {
        RemoveCustomType(nameof(CamShakeInducer));
        RemoveCustomType(nameof(Shaker));
    }
}

#endif