namespace Additions;

using System.Diagnostics;
using Debugging;
using Godot;

#if DEBUG

public static class Debug
{
    public static void AddWatcher(this Godot.Object target, NodePath property, Color? color = null, bool autoRemove = true, bool showTargetName = true, string optionalName = "") => DebugOverlay.instance?.AddWatcher(target, property, autoRemove, showTargetName, color, optionalName);
    public static void RemoveWatcher(this Godot.Object target, NodePath property) => DebugOverlay.instance?.RemoveWatcher(target, property);
    public static void RemoveWatchersWithTarget(this Godot.Object target) => DebugOverlay.instance?.RemoveWatchersWithTarget(target);
    public static void Log(this Godot.Object target, object message, float time = 2, Color? color = null, string optionalName = "", bool alsoPrint = true) => DebugOverlay.instance?.LogT(target, message.ToString(), time, false, color, optionalName, alsoPrint);
    public static void LogU(this Godot.Object target, object message, float time = 4, Color? color = null, string optionalName = "", bool alsoPrint = true) => DebugOverlay.instance?.LogT(target, message.ToString(), time, true, color, optionalName, alsoPrint);
    public static void LogFame(this Godot.Object target, object message, int frames = 1, Color? color = null, string optionalName = "", bool bottomLetf = false, bool alsoPrint = false) => DebugOverlay.instance?.LogFrame(target, message.ToString(), false, frames, color, optionalName, bottomLetf, alsoPrint);
    public static void LogPFrame(this Godot.Object target, object message, int physicFrames = 1, Color? color = null, string optionalName = "", bool bottomLetf = false, bool alsoPrint = false) => DebugOverlay.instance?.LogFrame(target, message.ToString(), true, physicFrames, color, optionalName, bottomLetf, alsoPrint);
}

#else

public static class Debug
{
    [Conditional("DEBUG")]
    public static void AddWatcher(this Godot.Object target, NodePath property, Color? color = null, bool autoRemove = true, bool showTargetName = true, string optionalName = "") { }
    [Conditional("DEBUG")]
    public static void RemoveWatcher(this Godot.Object target, NodePath property) { }
    [Conditional("DEBUG")]
    public static void RemoveWatchersWithTarget(this Godot.Object target) { }
    [Conditional("DEBUG")]
    public static void Log(this Godot.Object target, object message, float time = 2, Color? color = null, string optionalName = "", bool alsoPrint = true) { }
    [Conditional("DEBUG")]
    public static void LogU(this Godot.Object target, object message, float time = 4, Color? color = null, string optionalName = "", bool alsoPrint = true) { }
    [Conditional("DEBUG")]
    public static void LogFame(this Godot.Object target, object message, int frames = 1, Color? color = null, string optionalName = "", bool bottomLetf = false, bool alsoPrint = false) { }
    [Conditional("DEBUG")]
    public static void LogPFrame(this Godot.Object target, object message, int physicFrames = 1, Color? color = null, string optionalName = "", bool bottomLetf = false, bool alsoPrint = false) { }
}

#endif