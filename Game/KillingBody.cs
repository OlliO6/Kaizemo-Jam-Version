using System;
using System.Collections.Generic;
using System.Linq;
using Additions;
using Godot;

public class KillingBody : StaticBody2D
{
    [Export(PropertyHint.Layers2dPhysics)] public uint toKill;
    [Export] NodePath collisionObject;

    public KillingArea killArea = new();

    public override void _Ready()
    {
        AddChild(killArea);

        killArea.Monitorable = false;
        killArea.CollisionMask = toKill;

        killArea.AddChild(GetNode(collisionObject).Duplicate((int)DuplicateFlags.UseInstancing));
    }
}
