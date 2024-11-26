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
		地图生成器 = GetTree().Root.GetChild(0).GetNode<视差测试专用地图生成脚本>("地图生成器");
	}

	public override void _Process(double delta)
	{
	}

	public async void WalkTo(Vector3 targetRealPosition)
	{
		if(workerStatus != WorkerStatus.Walk && 地图生成器.Information.ContainsKey(targetRealPosition))
		{
			Vector3 start = CoordinateConverter.ToMatrixPosition(this.Position, int.Parse(GetParent<Node2D>().Name));
			Vector3 end = targetRealPosition;
			List<Vector2> path = new List<Vector2>();

			path = Astar.AstarAlgorithm(start, end, 地图生成器.Information);
			await Walking(path);
		}
	}

	private async Task Walking(List<Vector2> path)
	{
		await Task.Run(() =>{
			foreach (Vector2 NextStep in path)
			{
				CallDeferred("_SetPosition", NextStep);
				Thread.Sleep(500);
			}
			workerStatus = WorkerStatus.Idle;
		});
		
	}

	private void _SetPosition(Vector2 target)
	{
		Position = target;
	}
}
