using Godot;
using System;

public partial class 参数监控 : Label
{
	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
		Text = $"视差地图参数监控：\n帧率{Engine.GetFramesPerSecond()}";
	}
}
