using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;

public partial class 工人行为脚本 : Node2D
{
	public WorkerStatus workerStatus = WorkerStatus.Idle; // 工人状态
	public AnimatedSprite2D WorkerSprite; // 工人动画

	private 视差测试专用地图生成脚本 地图生成器;

	public override void _Ready()
	{
		WorkerSprite = GetNode<AnimatedSprite2D>("WorkerAnimated");
		地图生成器 = GetTree().Root.GetChild(0).GetNode<视差测试专用地图生成脚本>("地图生成器");
	}

	public override void _Process(double delta)
	{
		switch (workerStatus)
		{
			case WorkerStatus.Idle:
				ToIdleAnimation();
				break;
			case WorkerStatus.Crop:
				ToCropAnimation();
				break;
			case WorkerStatus.Walk:
				ToWalkAnimation();
				break;
		}
	}

	// FIXME: 未实现跨层移动
	public async void WalkTo(Vector3 target)
	{
		Astar astar = new Astar();
		astar.initstar(地图生成器);
		List<Vector3> path;

		path = astar.AStar(new Vector3(this.Position.X, this.Position.Y, int.Parse(GetParent<Node2D>().Name)), target, 地图生成器.Information);

		await Moving(path);
		workerStatus = WorkerStatus.Idle;
	}

	private Task Moving(List<Vector3> path)
	{
		workerStatus = WorkerStatus.Walk;
		foreach (Vector3 step in path)
		{
			Vector2 diraction = (坐标转换器.ToRealPosition(step) - this.Position).Normalized();

			while (this.Position != 坐标转换器.ToRealPosition(step))
			{
				// 插值向目标移动
            	this.Position += Position.Lerp(坐标转换器.ToRealPosition(step), 0.1f);
			}
		}
		return Task.CompletedTask;
	}

	private void ToIdleAnimation()
	{
		if (WorkerSprite.Animation != "Idle")
		{
			WorkerSprite.Animation = "Idle";
		}
	}

	private void ToCropAnimation()
	{
		if (WorkerSprite.Animation != "Crop")
		{
			WorkerSprite.Animation = "Crop";
		}

		// 具体行为
		
	}

	private void ToWalkAnimation()
	{
		if (WorkerSprite.Animation != "Walk")
		{
			WorkerSprite.Animation = "Walk";
		}
	}
}
