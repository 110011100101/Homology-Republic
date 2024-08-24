using Godot;
using System;

public partial class Block : Node2D
{
	public bool isWalkable; // 此方块上方是否允许行走
	public Vector3 BlockPosition; // 矩阵位置
	public Floor floor;
	public Ground ground;

	public override void _Ready()
	{
		// 这里写挂载逻辑
		floor = GetNode<Floor>("Floor");
		ground = GetNode<Ground>("Ground");
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
			floor.FloorMaterial = targetMaterial;
		}
	}

	// 更改地块材质
	public void ChangeGroundMaterial(GameMaterial targetMaterial)
	{
		if (targetMaterial != null)
		{
			// 这里写更改逻辑
			ground.GroundMaterial = targetMaterial;
		}
	}
}
