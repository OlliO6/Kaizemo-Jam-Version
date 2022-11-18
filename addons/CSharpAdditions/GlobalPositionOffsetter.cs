using Additions;
using Godot;

[Tool]
public class GlobalPositionOffsetter : Node2D
{
    [Export] private Vector2 offset, localOffset;
    [Export] private float rotationOffset;
    [Export] private bool syncRotation, updateOnPhysicsFrame;

    #region Parent Reference

    private Node2D storerForParent;
    public Node2D Parent => this.LazyGetNode(ref storerForParent, parentPath);
    [Export] private NodePath parentPath = "..";

    #endregion

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        if (!syncRotation)
        {
            RotationDegrees = rotationOffset;
        }
    }

    public override void _Process(float delta)
    {
        if (Engine.EditorHint)
        {
            UpdateTransformation();
            return;
        }

        if (!updateOnPhysicsFrame)
            UpdateTransformation();
    }
    public override void _PhysicsProcess(float delta)
    {
        if (updateOnPhysicsFrame)
            UpdateTransformation();
    }

    private void UpdateTransformation()
    {
        GlobalPosition = Parent.ToGlobal(localOffset) + offset;

        if (syncRotation)
            GlobalRotationDegrees = Parent.GlobalRotationDegrees + rotationOffset;
    }
}
