namespace Shaking;

using BetterInspector;
using Godot;

[Resource]
public partial class ShakeProfile : Resource
{
    [Export] public float amplitude = 3, time = 0.15f, speed = 1;
    [Export] public Curve amountCurve = GD.Load<Curve>("res://addons/Shake/DefaultAmountCurve.tres");
}
