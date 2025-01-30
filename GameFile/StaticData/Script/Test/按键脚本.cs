using Godot;
using System;
using RoseIsland.Library.CalculationTool.CoordinateConverter;
public partial class 按键脚本 : Node2D
{
	
	工人行为脚本 worker;
	视差地图相机专用脚本 camera;
	视差测试专用地图生成脚本 map;

	public override void _Ready()
	{
		camera = GetTree().Root.GetChild(0).GetNode<视差地图相机专用脚本>("Camera2D");
		map = GetTree().Root.GetChild(0).GetNode<视差测试专用地图生成脚本>("地图生成器");
	}

	public override void _Process(double delta)
	{
	}

	// E = 生成Worker
	// 上下移动滚轮 = 升高和降低层高度
	// 按住滚轮拖动 = 移动地图
	// 轻推W键 = 操练你的比
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey key)
		{
			if (Input.IsKeyPressed(Key.E))
			{
				worker = crateWorker();
			}	
		}
		if (@event is InputEventMouseButton mouseButton)
		{
			if (Input.IsMouseButtonPressed(MouseButton.Left))
			{
				worker.WalkTo(CoordinateConverter.ToMatrixPosition(new Vector2(GetGlobalMousePosition().X, GetGlobalMousePosition().Y), camera.cameraHight));
			}
			if (Input.IsMouseButtonPressed(MouseButton.Right))
			{
				GD.Print(CoordinateConverter.ToMatrixPosition(new Vector2(GetGlobalMousePosition().X, GetGlobalMousePosition().Y), camera.cameraHight));
			}
		}
	}

	public 工人行为脚本 crateWorker()
	{
		工人行为脚本 worker = (工人行为脚本)((PackedScene)ResourceLoader.Load("res://工程素材/脚本/预制体/工人/Worker.tscn")).Instantiate();
		worker.Position = new Vector2(0, 0);
		worker.Name = "大鸡鸡工人";
		map.GetNode("0").AddChild(worker);
		return worker;
	}
}
