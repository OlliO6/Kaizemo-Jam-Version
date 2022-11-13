using System.Collections.Generic;
using Additions;
using Godot;

public class GameManager : Node
{
    [Export] public Godot.Collections.Array<PackedScene> levels = new();
}
