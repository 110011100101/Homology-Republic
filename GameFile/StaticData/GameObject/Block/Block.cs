using Godot;
using System;

public partial class Block : Sprite2D
{
	public bool isWalkable; // 此方块上方是否允许行走
	public Vector3 BlockPosition; // 矩阵位置
	private GameMaterial floorMaterial;
	private GameMaterial FloorMaterial
	{
		get
		{
			return floorMaterial;
		}
		set
		{
			floorMaterial = value;

			// 更新材质图片
			Texture = (Texture2D)GD.Load(value.FloorTexturePath);
		}
	}
	private GameMaterial groundMaterial;
	private GameMaterial GroundMaterial
	{
		get
		{
			return groundMaterial;
		}
		set
		{
			groundMaterial = value;

			// 更新材质图片
			Texture = (Texture2D)GD.Load(value.GroundTexturePath);
		}
	}

	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
	}

	// 更改地板材质
	public void ChangeFloorMaterial(GameMaterial targetMaterial)
	{
		if (targetMaterial != null)
		{
			// 这里写更改逻辑
			FloorMaterial = targetMaterial;
		}
	}

	// 更改地块材质
	public void ChangeGroundMaterial(GameMaterial targetMaterial)
	{
		if (targetMaterial != null)
		{
			// 这里写更改逻辑
			GroundMaterial = targetMaterial;
		}
	}
}
