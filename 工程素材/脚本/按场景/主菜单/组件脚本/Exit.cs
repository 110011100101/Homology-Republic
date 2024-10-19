using Godot;
using System;

public partial class Exit : ChangeSceneButton
{
	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
	}

	public override void ChangeScene()
	{
		GetTree().Quit();
	}
}
