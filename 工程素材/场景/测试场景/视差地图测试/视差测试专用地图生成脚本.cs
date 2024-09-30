using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public partial class 视差测试专用地图生成脚本 : Node2D
{
	public List<上下文> Information {get; set;} = new List<上下文>();

	public override void _Ready()
	{
		for (int X = -6; X < 6; X++)
		for (int Y = -6; Y < 6; Y++)
		for (int Z = 0; Z < 10; Z++){
			// GD.Print($"正在生成{X},{Y},{Z}");
			MapCreater(new Vector3(X, Y, Z), new TestTile());
		}
	}

	public override void _Process(double delta)
	{
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
			// GD.Print("开始生成层");
			AddIndex(this, new Node2D() { Name = LevelName });
		}

		// block不存在
		// FIXME: 这里没有使用对象池
		if (!HasNode($"./{LevelName}/{BlockName}"))
		{
			上下文 context = Information.Find((上下文 x) => x.position == blockPosition);
			
			if (context == null)
			{
				Block block; // 预制体
				block = ((PackedScene)GD.Load("res://工程素材/脚本/预制体/Block/Block.tscn")).Instantiate<Block>();
				GetNode<Node2D>($"{LevelName}").AddChild(block);
				block.Position = 坐标转换器.ToRealPosition(blockPosition); // 位置
				block.Name = BlockName;
				block.ChangeGroundMaterial(GroundMaterial);
				block.ChangeFloorMaterial(FloorMaterial);

				// 更新上下文
				Information.Add(new 上下文(blockPosition, GroundMaterial, FloorMaterial));
			}
			else
			{
				Block block; // 预制体
				block = ((PackedScene)GD.Load("res://工程素材/脚本/预制体/Block/Block.tscn")).Instantiate<Block>();
				GetNode<Node2D>($"{LevelName}").AddChild(block);
				block.Position = new Vector2(context.position.X, context.position.Y); // 位置
				block.Name = $"{context.position.X},{context.position.Y}";
				block.ChangeGroundMaterial(context.GroundMaterial);
				block.ChangeFloorMaterial(context.FloorMaterial);
			}
			

			
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
			if(Parent.GetChildCount() == 0)
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
}