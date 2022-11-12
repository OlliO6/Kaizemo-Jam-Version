namespace Additions;

using System.Diagnostics;
using Debugging;
using Godot;

public static class Debug
{
    [Conditional("DEBUG")]
    public static void AddWatcher(this Godot.Object target, NodePath property, Color? color = null, bool autoRemove = true, bool showTargetName = true, string optionalName = "") => DebugOverlay.instance?.AddWatcher(target, property, autoRemove, showTargetName, color, optionalName);
    [Conditional("DEBUG")]
    public static void RemoveWatcher(this Godot.Object target, NodePath property) => DebugOverlay.instance?.RemoveWatcher(target, property);
    [Conditional("DEBUG")]
    public static void RemoveWatchersWithTarget(this Godot.Object target) => DebugOverlay.instance?.RemoveWatchersWithTarget(target);
    [Conditional("DEBUG")]
    public static void Log(this Godot.Object target, object message, float time = 2, Color? color = null, string optionalName = "", bool alsoPrint = true) => DebugOverlay.instance?.LogT(target, message.ToString(), time, false, color, optionalName, alsoPrint);
    [Conditional("DEBUG")]
    public static void LogU(this Godot.Object target, object message, float time = 4, Color? color = null, string optionalName = "", bool alsoPrint = true) => DebugOverlay.instance?.LogT(target, message.ToString(), time, true, color, optionalName, alsoPrint);
    [Conditional("DEBUG")]
    public static void LogFame(this Godot.Object target, object message, int frames = 1, Color? color = null, string optionalName = "", bool bottomLetf = false, bool alsoPrint = false) => DebugOverlay.instance?.LogFrame(target, message.ToString(), false, frames, color, optionalName, bottomLetf, alsoPrint);
    [Conditional("DEBUG")]
    public static void LogPFrame(this Godot.Object target, object message, int physicFrames = 1, Color? color = null, string optionalName = "", bool bottomLetf = false, bool alsoPrint = false) => DebugOverlay.instance?.LogFrame(target, message.ToString(), true, physicFrames, color, optionalName, bottomLetf, alsoPrint);
}
