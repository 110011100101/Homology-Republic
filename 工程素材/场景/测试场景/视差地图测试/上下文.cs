using Godot;
using System;

public partial class 上下文 : Node
{
    public Vector3 position;
    public GameMaterial GroundMaterial {get; set;}
    public GameMaterial FloorMaterial {get; set;}

    public 上下文(Vector3 position, GameMaterial groundMaterial, GameMaterial floorMaterial)
    {
        this.position = position;
        this.GroundMaterial = groundMaterial;
        this.FloorMaterial = floorMaterial;
    }
}
