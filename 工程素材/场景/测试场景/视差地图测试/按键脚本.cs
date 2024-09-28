using Godot;
using System;

public partial class 按键脚本 : Node2D
{
	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
		// 按下Q生成工人
		if (Input.IsKeyPressed(Key.Q))
		{
			Node2D worker = (Node2D)((PackedScene)ResourceLoader.Load("res://工程素材/脚本/预制体/工人/Worker.tscn")).Instantiate();
			worker.Position = new Vector2(0, 0);
			worker.Name = "大鸡鸡工人";
			AddChild(worker);
		}
	}
}
