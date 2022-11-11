namespace Shaking;

using System.Collections.Generic;
using System.Linq;
using Additions;
using BetterInspector;
using Godot;

public partial class CamShaker : Shaker
{
    [NodeRef] public Camera2D camera;

    public static List<CamShaker> insatnces = new();


    public override void _EnterTree()
    {
        insatnces.Add(this);
    }

    public override void _ExitTree()
    {
        insatnces.Remove(this);
    }

    public override void _Process(float delta)
    {
        amp = GetAmp();
        freqSummand = GetFreqSummand();
        ProcessNoise(delta);
        camera.Rotation = GetShakedRotation();
        camera.Offset = GetShakedPosition();
    }

    public static void ShakeAll(ShakeProfile profile) => ShakeAll(profile, 1f);
    public static void ShakeAll(ShakeProfile profile, float ampAndTimeFactor) => ShakeAll(profile, ampAndTimeFactor, ampAndTimeFactor);
    public static void ShakeAll(ShakeProfile profile, float ampFactor, float timeFactor)
    {
        if (profile is null) return;

        foreach (var camShaker in insatnces)
        {
            profile = GetUnusedProfile(profile);
            profile.Bound(camShaker);
            camShaker.currentShakeProfiles.Add(profile);
            profile.StartInterpolate(ampFactor, timeFactor);
            profile.Finished += (ShakeProfile shakeProfile) => camShaker.currentShakeProfiles.Remove(shakeProfile);
        }
    }
}
