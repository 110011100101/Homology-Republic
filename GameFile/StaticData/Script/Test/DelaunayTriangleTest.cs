using Godot;
using RoseIsland.Library.Algorithm.DelaunayTriangle;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

using Vector2 = Godot.Vector2;

public partial class DelaunayTriangleTest : Node2D
{
    private List<Vector2> points;

    public override void _Ready()
    {
        points = new List<Vector2>();
        Random random = new Random();

        GD.Print("开始生成点");

        // 手动添加点数据
        for (int i = 0; i < 15; i++)
        {
            float x = random.Next(1, 200);
            float y = random.Next(1, 200);
            points.Add(new Vector2(x, y));
        }

        foreach (Vector2 temp in points)
        {
            GD.Print($"点：{temp}");
        }

        int triangleSize = 200;

        Point point = DelaunayTriangle.Main(points, triangleSize);

        DrawNet(point, new List<Point>());
    }

    public override void _Process(double delta)
    {
    }

    public void DrawNet(Point point, List<Point> visitedPoints)
    {
        if (visitedPoints.Contains(point))
        {
            return;
        }

        visitedPoints.Add(point);
        GD.Print($"当前点：{point.Position}");

        Sprite2D sprite2D = new Sprite2D()
        {
            Name = $"{point.Position}",
            Position = new Godot.Vector2(point.Position.X, point.Position.Y),
            Texture = (Texture2D)GD.Load(TexturePath.GetFeaturePointTexturePath("GridConceptPack")),
            Scale = new Godot.Vector2(0.3f, 0.3f)
        };

        AddChild(sprite2D);

        foreach (Point neighbor in point.Neighbors)
        {
            DrawNet(neighbor, visitedPoints);
        }
    }
}