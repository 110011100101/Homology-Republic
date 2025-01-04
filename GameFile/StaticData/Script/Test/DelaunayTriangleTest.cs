using Godot;
using RoseIsland.Library.Algorithm.DelaunayTriangle;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Vector2 = System.Numerics.Vector2;

public partial class DelaunayTriangleTest : Node2D
{
    private List<Vector2> points;

    public override async void _Ready()
    {
        points = new List<Vector2>();

        GD.Print("开始生成点");

        // 手动添加点数据
        points.Add(new Vector2(400, 200)); // SuperA
        points.Add(new Vector2(-200, 200)); // SuperB
        points.Add(new Vector2(100, -300)); // SuperC
        points.Add(new Vector2(144, 196)); // A
        points.Add(new Vector2(56, 95)); // B
        points.Add(new Vector2(156, 153)); // C
        points.Add(new Vector2(22, 132)); // D

        foreach (Vector2 temp in points)
        {
            GD.Print($"点：{temp}");
        }

        int triangleSize = 200;

        await Task.Run(() => {
            Godot.Collections.Array godotPoints = new Godot.Collections.Array();
            foreach (var p in points)
            {
                godotPoints.Add(new Godot.Vector2(p.X, p.Y));
            }
            CallDeferred("操你的妈", godotPoints, triangleSize, this);
        });

        // DrawNet(point, new List<Point>());
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
            Texture = (Texture2D)GD.Load("res://GameFile/StaticData/GameAssets/Texture/GridConceptPack/tiles/FeaturePoints.png"),
            Scale = new Godot.Vector2(0.3f, 0.3f)
        };

        AddChild(sprite2D);

        foreach (Point neighbor in point.Neighbors)
        {
            DrawNet(neighbor, visitedPoints);
        }
    }
    
    public void 操你的妈(Godot.Collections.Array InVectors, int triangleSize, Node2D testNode)
    {
        DelaunayTriangle.Main(InVectors, triangleSize, testNode);
    }
}