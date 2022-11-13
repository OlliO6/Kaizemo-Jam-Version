using Godot;

public interface IDiveGainer
{
    public void GainDive();
}

public interface IHoldAndThrowable
{
    public Vector2 HoldOffset => Vector2.Zero;
    public bool IsPicked { get; set; }
    public bool CanExtendAirTime => true;
    public float ExtendAirVelocity => -140;
    public Node Holder { get; set; }

    public void Throw(Player.ActionDirection direction);
}

public interface IFalling
{
    public Vector2 Velocity { get; set; }
}