using Godot;
using System;
using System.Collections;

public partial class Floor : Sprite2D
{
	private GameMaterial floorMaterial;

	public GameMaterial FloorMaterial
	{
		get
		{
			return floorMaterial;
		}
		set
		{
			floorMaterial = value;

			// 更新材质图片
			Texture = (Texture2D)GD.Load(value.strFloorTexturePath);
		}
	}
	
	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
	}

}
