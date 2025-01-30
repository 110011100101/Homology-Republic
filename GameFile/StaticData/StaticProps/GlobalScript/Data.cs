using Godot;
using System;

public partial class Data : Node
{
	// 材质包数据
	[Export]
	public TileSet TexturePack;

	// 创建世界数据4
	[Export]
	public string PlanetName;
	[Export]
	public int MapSize;
	[Export]
	public float OceanToLandRatio;
	[Export]
	public int NumberOfPlates;

	// 预加载场景 
	[Export]
	public PackedScene MainMenu;
	[Export]
	public PackedScene SettingMenu;
	[Export]
	public PackedScene CreatePlanetMenu;
	[Export]
	public PackedScene BasePlanetCreatingMenu;
	[Export]
	public PackedScene PlayGround;
}
