using Godot;
using System;

public partial class 工人行为脚本 : Node2D
{
	public WorkerStatus workerStatus = WorkerStatus.Idle; // 工人状态
	public AnimatedSprite2D WorkerSprite; // 工人动画

	public override void _Ready()
	{
		WorkerSprite = GetNode<AnimatedSprite2D>("WorkerAnimated");
	}

	public override void _Process(double delta)
	{
		// 按下空格键切换工人状态(临时测试用)
		if (Input.IsActionJustPressed("ui_accept"))
		{
			if (workerStatus == WorkerStatus.Idle)
			{
				workerStatus = WorkerStatus.Croping;
			}
			else
			{
				workerStatus = WorkerStatus.Idle;
			}
		}

		// 工人状态机
		switch (workerStatus)
		{
			case WorkerStatus.Idle:
				Idle();
				break;
			case WorkerStatus.Croping:
				Crop();
				break;
		}
	}

	private void Idle()
	{
		if (WorkerSprite.Animation != "Idle")
		{
			WorkerSprite.Animation = "Idle";
		}
	}

	private void Crop()
	{
		if (WorkerSprite.Animation != "Crop")
		{
			WorkerSprite.Animation = "Crop";
		}

		// 具体行为
		
	}
}
