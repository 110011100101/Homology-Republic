using Godot;
using System;

public partial class ChangeSceneButton_CreatePlanetMenu_BackToMainMenu : ChangeSceneButton
{
	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
	}

	public override void ChangeScene()
	{
		GetTree().ChangeSceneToPacked(GetNode<Data>("/root/Data").MainMenu);
	}
}
