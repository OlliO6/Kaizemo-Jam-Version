using Godot;

public interface IDiveGainer
{
    public void GainDive();
}

public interface IHoldAndThrowable
{
    public Vector2 HoldOffset => Vector2.Zero;
    public bool IsPicked { get; set; }

    public void Throw(Player.ActionDirection direction);
}