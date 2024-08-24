using Godot;
using System;

public partial class Ground : Sprite2D
{
	private GameMaterial groundMaterial;
	public GameMaterial GroundMaterial
	{
		get
		{
			return groundMaterial;
		}
		set
		{
			groundMaterial = value;

			// 更新材质图片
			Texture = (Texture2D)GD.Load(value.strGroundTexturePath);
		}
	}

	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
	}
}
