using Godot;
using Godot.Collections;
using RoseIsland.Library.Algorithm.DelaunayTriangle;
using System;

using Color = Godot.Color;
using Vector2 = Godot.Vector2;
using SpriteList = System.Collections.Generic.List<Godot.Sprite2D>;
using VectorList = System.Collections.Generic.List<Godot.Vector2>;
using Pair = System.Collections.Generic.KeyValuePair<Block, Godot.Sprite2D>;

public partial class MapCreater : Node2D
{
	private Data data;
	private Notice_BasePlanetCreatingMenu Notice; // 这个链接到公告面板	
	// 颜色池
	private Array<Color> ColorPool = new Array<Color>() { 
		new Color(1.0f, 0.498f, 0.314f, 1.0f),		// 橙红色
		new Color(0.392f, 0.584f, 0.929f, 1.0f),	// 天蓝色
		new Color(0.769f, 0.306f, 0.804f, 1.0f),	// 紫罗兰色
		new Color(0.984f, 0.604f, 0.604f, 1.0f),	// 桃红色
	};

	public override void _Ready()
	{
		data = GetNode<Data>("/root/Data");
		Notice = GetNode<Notice_BasePlanetCreatingMenu>("/root/BasePlanetCreatingMenu/Notice");
		GlobalPosition = new Vector2(0, -(CoordinateConverter.ToRealPosition(new Vector2(data.MapSize, data.MapSize)).Y / 2)); // 把 MapSize 转成真实坐标后取一半
	}

	public override void _Process(double delta)
	{
	}

	public void Main()
	{
		Random rand = new Random(); 												// 随机数生成器
		SpriteList FeaturePoints = new SpriteList(); 								// 存放特征点
		Dictionary<Vector2, Color> ColorDic = new Dictionary<Vector2, Color>(); 	// 记录点和颜色的对应关系
		VectorList vectors = new VectorList(); 										// 导入德劳内三角的合法点集
		Dictionary<Block, Sprite2D> BlockBlones = new Dictionary<Block, Sprite2D>(); // 储存每个方块对应的特征点
		
		// 生成地基
		for (int i = 0; i < data.MapSize; i++)
		for (int j = 0; j < data.MapSize; j++)
			CreateBlock(new Vector2(i, j));

		// 选定特征点
		while (FeaturePoints.Count < data.NumberOfPltes)
		{
			Sprite2D FeaturePoint = new Sprite2D()
			{
				Position = CoordinateConverter.ToRealPosition(new Vector2(rand.Next(data.MapSize), rand.Next(data.MapSize))),
				Name = $"FeaturePoint{Position}",
				Texture = (Texture2D)GD.Load("res://GameFile/StaticData/GameAssets/Texture/GridConceptPack/tiles/FeaturePoint.png"), // TODO：这里没做材质包的动态适配,在那个文件夹的路径中插一个访问data里的材质就行
				Scale = new Vector2(0.1f, 0.1f)
			};

			if (FeaturePoints.Contains(FeaturePoint))
			{
				FeaturePoints.Add(FeaturePoint);
			}
		}

		Convergence(FeaturePoints, BlockBlones);

		// 接下来需要导出特征点归属集合
		// 然后清理线条和特征点,并且给每个block分配大陆并提供按钮以显示颜色区分的视图

		// TODO: 在这里采用贪心算法给德劳内三角剖分后输出的点分配颜色
		
		// 根据特征点清单构建点集

		foreach (Sprite2D sprite2D in FeaturePoints)
		{
			vectors.Add(new Vector2(sprite2D.Position.X, sprite2D.Position.Y));
		}

		// 分配: 
		AllocationColor(ColorDic, DelaunayTriangle.Main(vectors, data.MapSize));
		
		// 异步上色
		// TODO: 还未实现异步
		foreach (Node node in this.GetChildren())
		{
			if (node is Sprite2D)
			{
				// 声明一下是因为这是静态语言,不声明调用不了Block的属性
				Block block = (Block)node;
				block.Modulate = ColorDic[BlockBlones[block].Position];
			}
		}

		// 
		// 选配地形
		// 
		// 海陆划分
		
	}

	private void CreateBlock(Vector2 BlockPosition)
	{
		// init
		Block block = ((PackedScene)GD.Load(PrefebPath.BlockPath)).Instantiate<Block>();

		block.Position = CoordinateConverter.ToRealPosition(BlockPosition);
		block.Name = $"{BlockPosition}";
		this.AddChild(block);
	}

	// 收敛
	private void Convergence(SpriteList FeaturePoints, Dictionary<Block, Sprite2D> BlockBlone)
	{
		bool isConvergenced = false;
		Dictionary<Block, Sprite2D> BloneDic = new Dictionary<Block, Sprite2D>();

		while (!isConvergenced)
		{
			BloneDic = new Dictionary<Block, Sprite2D>();

			foreach (Node2D block in GetChildren())
			{
				if (block.GetType() == typeof(Block))
				{
					PickFeaturePoint(FeaturePoints, (Block)block, BloneDic);
				}
			}

			// 调整特征点然后决定是否继续遍历
			// 求出中心位置,然后把特征点放过去
			foreach (Sprite2D FeaturePoint in FeaturePoints)
			{
				if (FeaturePoint.Position != GetCenterPosition(BloneDic, FeaturePoint))
				{
					isConvergenced = false;
					break;
				}
				isConvergenced = true;
			}

			if (!isConvergenced)
			{
				foreach (Sprite2D FeaturePoint in FeaturePoints)
				{
					FeaturePoint.Position = GetCenterPosition(BloneDic, FeaturePoint);
				}
			}
		}
		BlockBlone = BloneDic;
	}

	/// <summary>
	///	选择特征点
	///	<span>这个方法会将传入的 <paramref name="block"/> 与传入的 <paramref name="FeaturePoints"/> 集合进行轮询</span>
	/// </summary>
	/// <param name="FeaturePoints">特征点集</param>
	/// <param name="block">block</param>
	private void PickFeaturePoint(SpriteList FeaturePoints, Block block, Dictionary<Block, Sprite2D> BloneDic)
	{
		Sprite2D FeatureBlone = null;

		foreach (Sprite2D FeaturePoint in FeaturePoints)
		{
			if (FeatureBlone == null)
			{
				FeatureBlone = FeaturePoint;
			}
			else if (block.Position.DistanceTo(FeatureBlone.Position) > block.Position.DistanceTo(FeaturePoint.Position))
			{
				// 比较距离
				FeatureBlone = FeaturePoint;
			}

			if (this.HasNode($"Line{block.Name}"))
			{
				GetNode<Line2D>($"Line{block.Name}").RemovePoint(1);
				GetNode<Line2D>($"Line{block.Name}").AddPoint(FeatureBlone.Position);
			}
			else
			{
				Line2D AFuckingLine = new Line2D()
				{
					Name = $"Line{block.Name}",
					Points = new Vector2[]{
						block.Position,
						FeatureBlone.Position
					},
					DefaultColor = new Color(1, 0, 0),
					Width = 1f,
				};
				this.AddChild(AFuckingLine);
			}
		}
		_UpdateDic<Block, Sprite2D>(block, FeatureBlone, BloneDic);
	}

	private void _UpdateDic<[MustBeVariant] T, [MustBeVariant] U>(T key, U value, Dictionary<T, U> dic)
	{
		U temp;

		if (dic.TryGetValue(key, out temp))
		{
			dic[key] = value;
		}
		else
		{
			dic.Add(key, value);
		}
	}

	private Vector2 GetCenterPosition(Dictionary<Block, Sprite2D> dictionary, Sprite2D FeaturePoint)
	{
		float sumX = 0;
		float sumY = 0;
		int count = 0;

		foreach (Pair pair in dictionary)
		{
			if (pair.Value == FeaturePoint)
			{
				sumX += pair.Key.Position.X;
				sumY += pair.Key.Position.Y;
				count++;
			}
		}

		float averageX = sumX / count;
		float averageY = sumY / count;

		return new Vector2(averageX, averageY);
	}

	/// <summary>
	///	此方法会自动通过起始点来遍历整个网,并分配颜色
	/// </summary>
	/// <param name="ColorDic">储存颜色与点的对应关系的字典</param>
	/// <param name="point">起始点</param>
	private void AllocationColor(Dictionary<Vector2, Color> ColorDic, Point point)
    {
        if (ColorDic.ContainsKey(new Vector2(point.Position.X, point.Position.Y)))
        {
            return;
        }

		foreach (Color color in ColorPool)
		foreach (Point neighbor in point.Neighbors)
		{
			if (ColorDic.ContainsKey(new Vector2(neighbor.Position.X, neighbor.Position.Y)))
			{
				if (ColorDic[new Vector2(neighbor.Position.X, neighbor.Position.Y)] != color)
				{
					ColorDic.Add(new Vector2(point.Position.X, point.Position.Y), color);
					return;
				}
			}
		}

        foreach (Point neighbor in point.Neighbors)
        {
            AllocationColor(ColorDic, neighbor);
        }
    }
	
}

