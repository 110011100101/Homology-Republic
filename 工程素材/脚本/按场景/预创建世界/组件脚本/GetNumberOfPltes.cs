using Godot;
using System;

public partial class GetNumberOfPltes : SpinBox
{
	public override void _Ready()
	{
		ChangeMaxValue(((float)GetNode<SpinBox>("/root/CreatePlanetMenu/MapInformationGroup/PlanetParameter/MapSize/GetMapSize").Value));
	}

	public override void _Process(double delta)
	{
	}

	public void ChangeMaxValue(float value)
	{
		MaxValue = Math.Pow(value, 2);
	}
}
