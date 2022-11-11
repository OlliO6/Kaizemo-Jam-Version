using System.Collections.Generic;
using Additions;
using Godot;
using Shaking;

public class CamShakeInducer : Node
{
    [Export] public ShakeProfile shakeProfile;

    public void Shake() => CamShaker.ShakeAll(shakeProfile);
    public void Shake(float ampAndTimeFactor) => CamShaker.ShakeAll(shakeProfile, ampAndTimeFactor);
    public void Shake(float ampFactor, float timeFactor) => CamShaker.ShakeAll(shakeProfile, ampFactor, timeFactor);
}
