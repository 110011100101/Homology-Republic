using System.Collections.Generic;
using Vector2 = Godot.Vector2;
using Color = Godot.Color;

public class Point
{
    public Vector2 Position;
    public List<Point> Neighbors;

    public Point(Vector2 position)
    {
        Position = position;
        Neighbors = new List<Point>();
    }
}