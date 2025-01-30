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

        DrawPoint(point, new List<Point>());
        DrawNet(point);
        
    }

    public override void _Process(double delta)
    {
    }

    public void DrawPoint(Point point, List<Point> visitedPoints)
    {
        if (visitedPoints.Contains(point))
        {
            return;
        }

        visitedPoints.Add(point);
        GD.Print($"当前点：{point.Position}");

        Sprite2D sprite2D = new Sprite2D();

        AddChild(sprite2D);

        foreach (Point neighbor in point.Neighbors)
        {
            DrawPoint(neighbor, visitedPoints);
        }
    }

	public List<Point> FinishPoints= new List<Point>();
    public void DrawNet(Point startPoint)
    {
        // 检查邻居的逻辑是否正确
		FinishPoints.Add(startPoint);

		foreach (Point neighbor in startPoint.Neighbors)
		{
			this.AddChild(new Line2D(){ Points = [startPoint.Position, neighbor.Position], Width = 0.3f });
		}
		
		foreach (Point neighbor in startPoint.Neighbors)
		{
			if (!FinishPoints.Contains(neighbor))
			{
				DrawNet(neighbor);
			}
		}
    }
}