using Godot;
using System;
using System.Net;

public partial class ChangeSceneButton_CreatePlanetMenu_ToBasePlanetCreatingMenu : ChangeSceneButton
{
	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
	}

	public override void ChangeScene()
	{
		UpdatePlanetParameters();
		GetTree().ChangeSceneToPacked(GetNode<Data>("/root/Data").BasePlanetCreatingMenu);
	}

	public void UpdatePlanetParameters()
	{
		Data data = GetNode<Data>("/root/Data");
		
	 	data.PlanetName = GetNode<LineEdit>("../MapInformationGroup/PlanetName/GetPlanetName").Text;
	 	data.MapSize = ((int)GetNode<SpinBox>("../MapInformationGroup/PlanetParameter/MapSize/GetMapSize").Value);
	 	data.OceanToLandRatio = ((float)GetNode<SpinBox>("../MapInformationGroup/PlanetParameter/OceanToLandRatio/GetOceanToLandRatio").Value);
	 	data.NumberOfPlates = ((int)GetNode<SpinBox>("../MapInformationGroup/PlanetParameter/NumberOfPltes/GetNumberOfPltes").Value);
	}
}
