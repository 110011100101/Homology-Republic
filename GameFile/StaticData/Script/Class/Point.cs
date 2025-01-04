using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

public class Point
{
    public Vector2 Position;
    public List<Point> Neighbors;
    public Color color;

    public Point(Vector2 position)
    {
        Position = position;
        Neighbors = new List<Point>();
        color = new Color();
    }
}