using Godot;
using Godot.Collections;
using RoseIsland.Library.Algorithm.DelaunayTriangle;
using RoseIsland.Library.CalculationTool.CoordinateConverter;
using System;
using System.Threading.Tasks;
using System.Linq;

using Color = Godot.Color;
using Vector2 = Godot.Vector2;
using SpriteList = System.Collections.Generic.List<Godot.Sprite2D>;
using VectorList = System.Collections.Generic.List<Godot.Vector2>;
using Pair = System.Collections.Generic.KeyValuePair<Block, Godot.Sprite2D>;
using List = System.Collections.Generic.List<Point>;


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

	public async void Main()
	{
		GD.Print("检查点1: 开始 Main 方法");
		await Task.Delay(1000);

		Random rand = new Random(); 													// 随机数生成器
		SpriteList FeaturePoints = new SpriteList(); 									// 存放特征点
		Dictionary<Vector2, Color> ColorDic = new Dictionary<Vector2, Color>(); 		// 记录点和颜色的对应关系
		VectorList vectors = new VectorList(); 											// 导入德劳内三角的合法点集
		Dictionary<Block, Sprite2D> BlockBlones = new Dictionary<Block, Sprite2D>(); 	// 储存每个方块对应的特征点

		GD.Print("检查点2: 生成地基");
		await Task.Delay(1000);

		// 生成地基
		for (int i = 0; i < data.MapSize; i++)
		{
			await Task.Run (() => {	
				for (int j = 0; j < data.MapSize; j++)
					CallDeferred(nameof(CreateBlock), new Vector2(i, j));
			});
		}

		GD.Print("检查点3: 选定特征点");
		await Task.Delay(1000);

		// 选定特征点
		while (FeaturePoints.Count < data.NumberOfPlates)
		{
			Sprite2D FeaturePoint = new Sprite2D()
			{
				Position = CoordinateConverter.ToRealPosition(new Vector2(rand.Next(data.MapSize), rand.Next(data.MapSize))),
				Name = $"FeaturePoint{Position}",
				Texture = (Texture2D)GD.Load(TexturePath.GetFeaturePointTexturePath(data.TexturePackName)),
				Scale = new Vector2(0.1f, 0.1f)
			};

			if (!FeaturePoints.Contains(FeaturePoint))
			{
				FeaturePoints.Add(FeaturePoint);
			}
		}

		GD.Print("检查点4: 收敛特征点");
		await Task.Delay(1000);

		// 收敛
		BlockBlones = Convergence(FeaturePoints);

		GD.Print("检查点5: 打印 BlockBlones");
		await Task.Delay(1000);

		// 打印 BlockBlones 的一个元素
		GD.Print($"BlockBlones Entry: Key = {BlockBlones.First().Key.Position}, Value = {BlockBlones.First().Value.Position}");

		GD.Print("检查点6: 根据特征点清单构建点集");
		await Task.Delay(1000);

		// 根据特征点清单构建点集
		foreach (Sprite2D sprite2D in FeaturePoints)
		{
			vectors.Add(new Vector2(sprite2D.Position.X, sprite2D.Position.Y));
		}

		GD.Print("检查点7: 分配颜色");
		await Task.Delay(1000);

		// 分配: 
		Point startPoint = DelaunayTriangle.Main(vectors, (int)CoordinateConverter.ToRealPosition(new Vector2(data.MapSize, data.MapSize)).X);
		AllocateColor(ColorDic, startPoint);

		// 打印 ColorDic 的所有元素
		foreach (var entry in ColorDic)
		{
		    GD.Print($"ColorDic Entry: Key = {entry.Key}, Value = {entry.Value}");
		}

		GD.Print("检查点8: 异步上色");
		await Task.Delay(1000);

		// 异步上色
		await Task.Run (() => {
			CallDeferred(nameof(UpdateColor), ColorDic, BlockBlones);
		});

		GD.Print("检查点9: 地形配置");
		await Task.Delay(1000);

		// 
		// 选配地形
		// 
		// 海陆划分
		GD.Print("检查点10: 开始地形配置");
		await Task.Delay(1000);

		// 检查并修正重复颜色
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
	private Dictionary<Block, Sprite2D> Convergence(SpriteList FeaturePoints)
	{
		bool isConvergenced = false;
		Dictionary<Block, Sprite2D> BlockBlones = new Dictionary<Block, Sprite2D>(); // 修改注释: BloneDic -> BlockBlones

		while (!isConvergenced)
		{
			BlockBlones = new Dictionary<Block, Sprite2D>();

			foreach (Node2D block in GetChildren())
			{
				if (block.GetType() == typeof(Block))
				{
					PickFeaturePoint(FeaturePoints, (Block)block, BlockBlones);
				}
			}

			// 调整特征点然后决定是否继续遍历
			// 求出中心位置,然后把特征点放过去
			foreach (Sprite2D FeaturePoint in FeaturePoints)
			{
				if (FeaturePoint.Position != GetCenterPosition(BlockBlones, FeaturePoint))
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
					FeaturePoint.Position = GetCenterPosition(BlockBlones, FeaturePoint);
				}
			}
		}
		return BlockBlones;
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
		UpdateDic<Block, Sprite2D>(block, FeatureBlone, BloneDic);
	}

	private void UpdateDic<[MustBeVariant] T, [MustBeVariant] U>(T key, U value, Dictionary<T, U> dic)
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

	public void UpdateColor(Dictionary<Vector2, Color> ColorDic, Dictionary<Block, Sprite2D> BlockBlones)
	{
		foreach (Node node in this.GetChildren())
		{	
			if (node is Block)
			{
				Block block = (Block)node;
				if (BlockBlones.TryGetValue(block, out Sprite2D featurePoint) && ColorDic.TryGetValue(featurePoint.Position, out Color color))
				{
					block.Modulate = color;
				}
				else
				{
					GD.Print(featurePoint.Position,"颜色不存在");
				}
			}	

			if (node is Line2D)
			{
				this.RemoveChild(node);
			}
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
	/// 此方法会自动通过起始点来遍历整个网,并分配颜色，确保没有重复颜色
	/// </summary>
	/// <param name="ColorDic">储存颜色与点的对应关系的字典</param>
	/// <param name="point">需要分配的点</param>
	private void AllocateColor(Dictionary<Vector2, Color> ColorDic, Point point)
	{
		Point NextPoint = null;

		// 调用时分配给自己一个颜色
		foreach (Color color in ColorPool)
		{
			bool isRepeat = false;
			foreach (Point neighbor in point.Neighbors)
			{
				if (ColorDic.ContainsKey(neighbor.Position) && ColorDic[neighbor.Position].Equals(color))
				{
					isRepeat = true;
					break;
				}
			}
			
			if (!isRepeat)
			{
				ColorDic.Add(point.Position, color);
				break;
			}
		}

		// 决定递归对象
		foreach (Point p in point.Neighbors)
		{
			if (!ColorDic.ContainsKey(p.Position))
			{
				NextPoint = p;
			}
		}

		if (NextPoint != null)
		{
			AllocateColor(ColorDic, NextPoint);
		}
    }
}

