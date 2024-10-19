using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

public partial class 视差测试专用地图生成脚本 : Node2D
{
	public Dictionary<Vector3,上下文> Information { get; set; } = new Dictionary<Vector3, 上下文>();

	public List<Block> BlocksPrefab { get; set; } = new List<Block>();
	/// <summary>
	/// 预制体队列编号
	/// <para><b>解释:</b>此值用于按顺序使用预制体</para>
	/// </summary>
	private int PrefabQueueNumber = 0;

	/// <summary>
	/// 启用预制体的数量
	/// </summary>
	[Export] private int prefabCount = 0;
	[Export] private int int_x;
	[Export] private int int_y;
	[Export] private int int_z;


	public override void _Ready()
	{
		for (int i = 0; i < prefabCount; i++)
			BlocksPrefab.Add(((PackedScene)GD.Load("res://工程素材/脚本/预制体/Block/Block.tscn")).Instantiate<Block>());
		Task.Run(() =>
		{
			// temp
			for (int z = 0; z < int_z; z++)
				for (int y = 0; y < int_y; y++)
					for (int x = 0; x < int_x; x++)
					{
						CallDeferred("DefferMapCreater", new Vector3(x, y, z));
						Thread.Sleep(5);
					}
		});
	}

	public override void _Process(double delta)
	{
	}

	public void DefferMapCreater(Vector3 position){
		MapCreater(position, new wood());
	}

	/// <summary>
	/// 地图生成器
	/// </summary>
	/// <param name="blockPosition">位置</param>
	/// <param name="GroundMaterial">Ground 材质</param>
	/// <param name="FloorMaterial">Floor 材质</param>
	public void MapCreater(Vector3 blockPosition, GameMaterial GroundMaterial = null, GameMaterial FloorMaterial = null)
	{
		string LevelName = $"{blockPosition.Z}";
		string BlockName = $"{blockPosition.X},{blockPosition.Y}";

		// 层不存在
		if (!HasNode($"./{LevelName}"))
		{
			AddIndex(this, new Node2D() { Name = LevelName });
		}

		// block不存在
		if (!HasNode($"./{LevelName}/{BlockName}"))
		{
            Block block = BlocksPrefab[PrefabQueueNumber];
            Information.TryGetValue(blockPosition, out 上下文 context);

			if (block.IsInsideTree())
				block.GetParent().RemoveChild(block);

			if (context == null)
			{
				InitBlock(block, BlockName, blockPosition, GroundMaterial, FloorMaterial);
				UpdateContext(blockPosition, GroundMaterial, FloorMaterial);
			}
			else
			{
				InitBlock(block, $"{context.position.X},{context.position.Y}", new Vector2(context.position.X, context.position.Y), context.GroundMaterial, context.FloorMaterial);
			}
			GetNode<Node2D>($"{LevelName}").AddChild(block);
			UpdatePrefabeQueueNumber();
		}
	}

	/// <summary>
	/// 检查子节点是否存在于父节点中
	/// </summary>
	/// <param name="Parent">父节点</param>
	/// <param name="Child">子节点</param>
	// public bool IsNodeInAsChild(Node2D Parent, string Child)
	// {
	// 	// 尝试访问子节点
	// 	try
	// 	{
	// 		Parent.GetNode<Node2D>(Child);

	// 		GD.Print("IsNodeInAsChild(): return true"); 
	// 		return true;
	// 	}
	// 	catch (Exception e)
	// 	{
	// 		GD.Print("Error:" + e.Message);

	// 		GD.Print("IsNodeInAsChild(): return false");
	// 		return false;
	// 	}
	// }

	/// <summary>
	/// 向指定索引添加节点
	/// <para>特殊情况: 不能传入名字不是数字的节点,特别是不要有符号</para>
	/// </summary>
	/// <param name="Parent">父节点</param>
	/// <param name="LevelName">子节点名称</param>
	/// <param name="AutoIndexMode">自动生成索引模式</param>
	public void AddIndex(Node2D Parent, Node2D Child, bool AutoIndexMode = true, int Index = 0)
	{
		if (AutoIndexMode)
		{
			// 特殊情况处理
			if (Parent.GetChildCount() == 0)
			{
				Parent.AddChild(Child);
			}
			else if (HasNode($"{Parent.GetPath()}/{int.Parse(Child.Name) - 1}"))
			{
				AddChild(Child);
				MoveChild(Child, Parent.GetNode<Node2D>($"{int.Parse(Child.Name)}").GetIndex() + 1); // 升序
			}
			else
			{
				AddChild(Child);
				MoveChild(Child, 0);
			}
		}
		else
		{
			Parent.AddChild(Child);
			MoveChild(Child, Index);
		}
	}

	/// <summary>
	/// 更新上下文
	/// </summary>
	/// <param name="blockPosition">block 的像素级位置</param>
	/// <param name="GroundMaterial">Ground 材质</param>
	/// <param name="FloorMaterial">Floor 材质</param>
	public void UpdateContext(Vector3 blockPosition, GameMaterial GroundMaterial, GameMaterial FloorMaterial)
	{
		Information.Add(blockPosition, new 上下文(blockPosition, GroundMaterial, FloorMaterial));
	}

	/// <summary>
	/// 移除上下文
	/// </summary>
	/// <param name="blockPosition">block 的像素级位置</param>
	public void RemoveContext(Vector3 blockPosition)
	{
		Information.Remove(blockPosition);
	}

	/// <summary>
	/// 初始化block
	/// <para><b>重载:</b>接受 vector3 经过转换赋值给 <paramref name="blockPosition"/></para>
	/// </summary>
	/// <param name="block">需要初始化的block</param>
	/// <param name="BlockName">block 名称</param>
	/// <param name="blockPosition">block 的像素级坐标</param>
	/// <param name="GroundMaterial">Ground 材质</param>
	/// <param name="FloorMaterial">Floor 材质</param>
	public void InitBlock(Block block, string BlockName, Vector3 blockPosition, GameMaterial GroundMaterial, GameMaterial FloorMaterial)
	{
		block.Name = BlockName;
		block.Position = 坐标转换器.ToRealPosition(blockPosition);
		block.ChangeGroundMaterial(GroundMaterial);
		block.ChangeFloorMaterial(FloorMaterial);
	}

	/// <summary>
	/// 初始化block
	/// <para><b>重载:</b>接受 vector2 并直接赋值给 <paramref name="blockPosition"/></para>
	/// </summary>
	/// <param name="block">需要初始化的block</param>
	/// <param name="BlockName">block 名称</param>
	/// <param name="blockPosition">block 的像素级坐标</param>
	/// <param name="GroundMaterial">Ground 材质</param>
	/// <param name="FloorMaterial">Floor 材质</param>
	public void InitBlock(Block block, string BlockName, Vector2 blockPosition, GameMaterial GroundMaterial, GameMaterial FloorMaterial)
	{
		block.Name = BlockName;
		block.Position = blockPosition;
		block.ChangeGroundMaterial(GroundMaterial);
		block.ChangeFloorMaterial(FloorMaterial);
	}

	/// <summary>
	/// 更新预制体队列编号
	/// </summary>
	public void UpdatePrefabeQueueNumber()
	{
		if (PrefabQueueNumber < BlocksPrefab.Count - 1)
			PrefabQueueNumber++;
		else
			PrefabQueueNumber = 0;
	}
}