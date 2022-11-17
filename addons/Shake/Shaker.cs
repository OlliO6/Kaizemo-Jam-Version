namespace Shaking;

using System;
using System.Collections.Generic;
using System.Linq;
using Additions;
using Godot;

public class Shaker : Node2D
{
    [Export] public float rotationMagnitude = 10, positionMagnitude = 10, speed = 10, amountThreshold = 0.1f;
    [Export] public bool looping, shakeOnReady;
    [Export] public ShakeProfile defaultProfile = new();

    public OpenSimplexNoise noiseMap;

    protected List<RuntimeShakeProfile> runningShakeProfiles = new();
    protected float amp, freqFactor, noiseX;

    public bool IsShaking => runningShakeProfiles.Count > 0;

    public override void _Ready()
    {
        noiseMap = new OpenSimplexNoise()
        {
            Seed = (int)GD.Randi(),
            Period = 4,
            Octaves = 2,
        };

        if (shakeOnReady)
            Shake();
    }

    public void Shake(float ampAndTimeFactor, ShakeProfile profile = null) => Shake(profile, ampAndTimeFactor, ampAndTimeFactor);
    public void Shake(ShakeProfile profile = null, float ampFactor = 1, float timeFactor = 1)
    {
        if (profile is null) profile = defaultProfile;

        runningShakeProfiles.Add(new RuntimeShakeProfile(profile, ampFactor));
    }

    public void Stop()
    {
        for (int i = 0; i < runningShakeProfiles.Count;)
        {
            runningShakeProfiles[i].Free();
            runningShakeProfiles.RemoveAt(i);
        }
    }

    public override void _Process(float delta)
    {
        ProcessNoise(delta);
        ProcessProfiles(delta);

        amp = GetAmp();
        freqFactor = GetFreqFactor();

        ApplyShakedTransform(GetShakedPosition(), GetShakedRotation());
    }

    protected virtual void ApplyShakedTransform(Vector2 position, float rotation)
    {
        Position = position;
        Rotation = rotation;
    }

    protected void ProcessProfiles(float delta)
    {
        for (int i = 0; i < runningShakeProfiles.Count;)
        {
            RuntimeShakeProfile profile = runningShakeProfiles[i];

            profile.progress += delta;

            if (profile.progress >= 1)
            {
                if (!looping)
                {
                    profile.Free();
                    runningShakeProfiles.RemoveAt(i);
                    continue;
                }

                profile.progress = 0;
            }

            i++;
        }
    }

    protected void ProcessNoise(float delta) => noiseX += speed * freqFactor * delta;

    protected float GetFreqFactor() => runningShakeProfiles.Sum(profile => profile.FrequencyFactor);

    protected float GetAmp() => runningShakeProfiles.Sum(profile => profile.ShakeAmount);

    protected Vector2 GetShakedPosition() => amp < amountThreshold ? Vector2.Zero :
                    (positionMagnitude * amp * new Vector2(GetNoiseMagnitude(0), GetNoiseMagnitude(1)));
    protected float GetShakedRotation() => rotationMagnitude is 0 ? 0 :
                    (amp < amountThreshold ? 0 : rotationMagnitude * amp * GetNoiseMagnitude(2));

    // Returns from -1 to 1
    private float GetNoiseMagnitude(int layer) => noiseMap.GetNoise2d(layer * 100, noiseX);

    protected class RuntimeShakeProfile : Godot.Object
    {
        public float amplitude, speed;
        public float ampFactor;
        public float progress;
        public Curve amountCurve;

        public float ShakeAmount => amountCurve.InterpolateBaked(progress) * amplitude * ampFactor;
        public float FrequencyFactor => amountCurve.InterpolateBaked(progress) * speed;

        public RuntimeShakeProfile(ShakeProfile profile, float ampFactor)
        {
            amplitude = profile.amplitude;
            speed = profile.speed;
            this.ampFactor = ampFactor;
            amountCurve = profile.amountCurve;
            progress = 0;
        }
    }
}
