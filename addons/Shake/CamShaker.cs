namespace Shaking;

using System.Collections.Generic;
using BetterInspector;
using Godot;

public partial class CamShaker : Shaker
{
    [NodeRef] public Camera2D camera;

    public static List<CamShaker> insatnces = new();

    public override void _EnterTree() => insatnces.Add(this);

    public override void _ExitTree() => insatnces.Remove(this);

    public static void ShakeAll(ShakeProfile profile = null, float ampFactor = 1, float timeFactor = 1)
    {
        foreach (var camShaker in insatnces)
        {
            camShaker.Shake(profile, ampFactor, timeFactor);
        }
    }

    protected override void ApplyShakedTransform(Vector2 position, float rotation)
    {
        camera.Offset = position;
        camera.Rotation = rotation;
    }
}
