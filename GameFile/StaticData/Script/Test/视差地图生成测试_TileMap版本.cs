using Godot;
using System;

public partial class 视差地图生成测试_TileMap版本 : Node
{
	public override void _Ready()
	{
		Main();
	}

	public override void _Process(double delta)
	{
	}

	public void Main()
	{
		for (int level = 0; level < 100; level++)
		{
			// 导入包
			TileMapLayer map = new TileMapLayer() 
			{ 
				Name = $"{level}",
				ZIndex = level,
				TileSet = ResourceLoader.Load<TileSet>("res://GameFile/StaticData/GameAssets/Texture/GridConceptPack/GridConceptPack.tres")
					
			};
	
			// 加入树
			AddChild(map);
	
			// 加入一些图快
			for (int x = 0; x < 100; x++)
			for (int y = 0; y < 100; y++)
			{
				map.SetCell(new Vector2I(x, y), 3, new Vector2I(0, 0));
			}
		}
	}
}
