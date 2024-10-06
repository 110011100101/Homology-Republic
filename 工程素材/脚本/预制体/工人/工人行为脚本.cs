using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

public partial class 工人行为脚本 : Node2D
{
	public AnimatedSprite2D WorkerSprite; // 工人动画
	public WorkerStatus workerStatus = WorkerStatus.Idle; // 工人状态

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
				_ToIdleAnimation();
				break;
			case WorkerStatus.Crop:
				_ToCropAnimation();
				break;
			case WorkerStatus.Walk:
				_ToWalkAnimation();
				break;
		}
	}

	public async void WalkTo(Vector3 targetRealPosition)
	{
		Vector3 start = 坐标转换器.ToMatrixPosition(this.Position, int.Parse(GetParent<Node2D>().Name));
		Vector3 end = targetRealPosition;
		List<Vector2> path = new List<Vector2>();

		path = Astar.AstarAlgorithm(start, end, 地图生成器.Information);
		await Walking(path);
	}

	private Vector2 stepLength;
	private bool isArrived;
	private async Task Walking(List<Vector2> path)
	{
		
		await Task.Run(() =>{
			workerStatus = WorkerStatus.Walk;
				foreach (Vector2 NextStep in path)
				{
					isArrived = false;
					CallDeferred("_CountStepLength", NextStep);
					while (!isArrived)
					{
						CallDeferred("_UpdatePosition", stepLength, NextStep);
						CallDeferred("_isArrived", new Vector2(NextStep.X, NextStep.Y));
						Thread.Sleep(10);
					}
					CallDeferred("_SetPosition", NextStep);
				}
			workerStatus = WorkerStatus.Idle;
		});
		
	}

	private void _CountStepLength(Vector3 targetRealPosition)
	{
		float StepX = (targetRealPosition.X - Position.X) / (int)(Engine.GetFramesPerSecond() * 0.36);
		float StepY = (targetRealPosition.Y - Position.Y) / (int)(Engine.GetFramesPerSecond() * 0.36);

		stepLength = new Vector2(StepX,StepY);
	}

	private void _isArrived(Vector2 targetRealPosition)
	{
		isArrived = Math.Abs(Position.X - targetRealPosition.X) < 3f && Math.Abs(Position.Y - targetRealPosition.Y) < 3f;
	}

	private void _ToIdleAnimation()
	{
		if (WorkerSprite.Animation != "Idle")
		{
			WorkerSprite.Animation = "Idle";
		}
	}

	private void _ToCropAnimation()
	{
		if (WorkerSprite.Animation != "Crop")
		{
			WorkerSprite.Animation = "Crop";
		}
	}

	private void _ToWalkAnimation()
	{
		if (WorkerSprite.Animation != "Walk")
		{
			WorkerSprite.Animation = "Walk";
		}
	}

	private void _UpdatePosition(Vector2 steps, Vector2 target)
	{
		
		GD.Print("我在跑我在跑");
		GD.Print("stepLength: " + steps);
		GD.Print("Position: " + Position);
		GD.Print("target: " + target);
		Position += steps;
	}

	private void _SetPosition(Vector2 target)
	{
		Position = target;
	}
}
