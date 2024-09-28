using Godot;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public partial class 视差测试专用地图生成脚本 : Node2D
{

	public override void _Ready()
	{
		for (int X = 0; X < 10; X++)
		for (int Y = 0; Y < 10; Y++)
		for (int Z = 0; Z < 10; Z++){
			GD.Print($"正在生成{X},{Y},{Z}");
			MapCreater(new Vector3(X, Y, Z), new TestTile());
		}
	}

	public override void _Process(double delta)
	{
	}

	// 循环生成地图
	public void MapCreater(Vector3 blockPosition, GameMaterial GroundMaterial = null, GameMaterial FloorMaterial = null)
	{
		string LevelName = $"{blockPosition.Z}";
		string BlockName = $"{blockPosition.X},{blockPosition.Y}";

		// 层不存在
		if (!HasNode($"./{LevelName}"))
		{
			GD.Print("开始生成层");
			AddIndex(this, new Node2D() { Name = LevelName });
		}

		// block不存在
		if (!HasNode($"./{LevelName}/{BlockName}"))
		{
			Block block; // 预制体
			block = ((PackedScene)GD.Load("res://工程素材/脚本/预制体/Block/Block.tscn")).Instantiate<Block>();
			GD.Print("开始生成block");
			GetNode<Node2D>($"{LevelName}").AddChild(block);
			block.Position = 坐标转换器.ToRealPosition(blockPosition); // 位置
			block.Name = BlockName;
			block.ChangeGroundMaterial(GroundMaterial);
			block.ChangeFloorMaterial(FloorMaterial);

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
	// 	// FIXME: 此方法报错后并未执行生成层的代码
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