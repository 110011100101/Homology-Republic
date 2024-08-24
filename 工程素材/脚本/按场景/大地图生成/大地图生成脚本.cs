using Godot;
using System;

public partial class 大地图生成脚本 : Node2D
{
	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
	}

	// 生成大地图
	public void CreatWorld(int mapSize, int plateNum)
	{
		// --= 1.框架勾勒 =--
		// 生成地基(int 边长)
		//
		// 板块划分(int 板块数量)
		// 依据 区块数量 放置特征点
		// 收敛
		// 附加要求：要能设定特征点可以不可见
		// 附加要求：要能设定特征点可以不可交互
		// 附加要求：当特征点可见时要能设置连线可不可见
		// 
		// 
	}

	// 生成地基
	public void CreateBaseMap(int mapSize)
	{
		// -- 步骤: 生成 --
		// 根据边长<建立>二维循环
		// 循环内:
		// 		<初始化>一个地块, 补全地块所需要的参数
		// 		== 名称 ==
		// 		== 矩阵位置 ==
		// 		== 实际位置 ==
		//		加入本层并赋值给block
		// 		-- 更改材质 --
		// 		调用block的更改材质参数给地块改材质, ground改成earth
		//		
		
	}
}
