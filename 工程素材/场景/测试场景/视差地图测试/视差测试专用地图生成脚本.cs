using Godot;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

//
// 视差测试专用地图生成脚本
// 因为是测试所以没能控制每一个block的材质，实际上
// 

public partial class 视差测试专用地图生成脚本 : Node2D
{
	public override void _Ready()
	{
		// FIXME: 卡烂了，需要优化
		// PS: 必须从零开始生成,我不想写适配的索引,太麻烦,而且也用不上
		for(int i = 0; i <= 10; i++)
		{
			LoopMapCreater(6, i, null, new TestTile());
		}
	}

	public override void _Process(double delta)
	{
	}

	// 循环生成地图
	// 参数:地图边长, 目标高度, 预设材质 = null
	public void LoopMapCreater(int mapSize, int targetHeight, GameMaterial GroundMaterial = null, GameMaterial FloorMaterial = null)
	{
		Node2D HeightNode; // 高度层
		Block block; // 临时储存地块方便访问

		//////////////////////////////////////////////////////////////
		// 实现思路：
		//
		// -- 步骤: 预备 --
		// 扫描高度树
		// 如果没有目标高度的父节点，则<新建>一个然后赋值给HeightNode
		// 如果有则直接赋值给HeightNode
		// 
		// -- 步骤: 生成 --
		// 根据边长<建立>二维循环
		// 循环内:
		// 		扫描本层树, 检查是否目标位置已有Block
		// 		没有则, <初始化>一个地块, 补全地块所需要的参数
		//		加入本层并赋值给block
		// 		-- 更改材质 --
		// 		调用block的更改材质参数给地块改材质, Floor 和 Ground 都要改
		// 
		// -- 步骤: 结束 --
		// 结束
		//////////////////////////////////////////////////////////////

		//
		//
		//
		// -- 步骤: 预备 --
		// 扫描高度树
		// 如果没有目标高度的父节点，则<新建>一个然后赋值给HeightNode
		// 如果有则直接赋值给HeightNode
		if (IsNodeInAsChild(this, $"{targetHeight}") && targetHeight >= 0 && targetHeight <= 500)
		{
			HeightNode = this.GetNode<Node2D>($"{targetHeight}");
		}
		else
		{
			HeightNode = new Node2D();
			HeightNode.Name = $"{targetHeight}";
			this.AddChild(HeightNode);
		}
		//
		// -- 步骤: 生成 --
		// 根据边长<建立>二维循环
		// 循环内:
		// 		扫描本层树, 检查是否目标位置已有Block
		// 		没有则, <初始化>一个地块, 补全地块所需要的参数
		//		加入本层并赋值给block
		// 		-- 更改材质 --
		// 		调用block的更改材质参数给地块改材质, Floor 和 Ground 都要改
		for (int x = -mapSize; x < mapSize; x++)
			for (int y = -mapSize; y < mapSize; y++)
			{
				if (!IsNodeInAsChild(HeightNode, $"({x},{y},{targetHeight})") && (!(x == 0 && y == 0 && targetHeight == 10)))
				{
					// 加载场景, 然后用PackedScene.Instantiate()生成
					// FIXME: 这里其实不能直接放路径, 但是是测试, 就算了
					block = ((PackedScene)GD.Load("res://工程素材/脚本/预制体/Block/Block.tscn")).Instantiate<Block>();
					// 补全地块所需要的参数:
					// 名称--
					block.Name = $"({x},{y},{targetHeight})";

					// 瓦片坐标
					block.GetNode<Label>("瓦片坐标").Text = block.Name;

					// 矩阵位置--
					block.BlockPosition = new Vector3(x, y, targetHeight);
					// 实际位置--
					block.Position = 坐标转换器.ToRealPosition(block.BlockPosition);


					// 加入本层
					// TODO: 这里需要决定顺序
					HeightNode.AddChild(block);

					// GD.Print($"floor:{block.floor.Name}\nGround:{block.ground.Name}"); // 用来检测是否正确挂载了子节点
					// 更改材质--
					block.ChangeGroundMaterial(GroundMaterial);
					block.ChangeFloorMaterial(FloorMaterial);
				}
			}
		// -- 步骤: 结束 --
		// 结束
	}

	public bool IsNodeInAsChild(Node2D fatherNode, string ChildNodeToFind)
	{
		// 比对每一个子节点
		foreach (Node2D child in this.GetChildren())
		{
			if ($"{child.Name}" == ChildNodeToFind)
			{
				return true;
			}
		}
		return false;
	}
}
