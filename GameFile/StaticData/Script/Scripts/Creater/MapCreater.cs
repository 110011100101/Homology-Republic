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
using PointList = System.Collections.Generic.List<Point>;
using ColorList = System.Collections.Generic.HashSet<Godot.Color>;
using Pair = System.Collections.Generic.KeyValuePair<Block, Godot.Sprite2D>;

public partial class MapCreater : Node2D
{
	private Data data;
	public Dictionary<Vector2, Color> ColorDic = new Dictionary<Vector2, Color>();      // 记录点和颜色的对应关系

	// 颜色池
	private ColorList ColorPool = new ColorList() {
		new Color(1.0f, 0.498f, 0.314f, 1.0f),		// 橙红色
		new Color(0.392f, 0.584f, 0.929f, 1.0f),	// 天蓝色
		new Color(0.769f, 0.306f, 0.804f, 1.0f),	// 紫罗兰色
		new Color(0.984f, 0.604f, 0.604f, 1.0f),	// 桃红色
	};

	public override void _Ready()
	{
		data = GetNode<Data>("/root/Data");
		GlobalPosition = new Vector2(0, -(CoordinateConverter.ToRealPosition(new Vector2(data.MapSize, data.MapSize)).Y / 2)); // 把 MapSize 转成真实坐标后取一半
	}

	public override void _Process(double delta)
	{
	}

	public async void Main()
	{
		Random rand = new Random();                                                     // 随机数生成器
		SpriteList featurePoints = new SpriteList();                                    // 存放特征点
		VectorList vectors = new VectorList();                                          // 导入德劳内三角的合法点集
		Dictionary<Block, Sprite2D> blockBlones = new Dictionary<Block, Sprite2D>();    // 储存每个方块对应的特征点
		Dictionary<Sprite2D, int> blockNumber = new Dictionary<Sprite2D, int>();        // 储存每个区块的方格数量

		// 生成地基
		for (int i = 0; i < data.MapSize; i++)
			await Task.Run(() =>
			{
				for (int j = 0; j < data.MapSize; j++)
					CallDeferred(nameof(CreateBlock), new Vector2(i, j));
			});

		// 选定特征点
		while (featurePoints.Count < data.NumberOfPlates)
		{
			Sprite2D featurePoint = new Sprite2D()
			{
				Position = CoordinateConverter.ToRealPosition(new Vector2(rand.Next(data.MapSize), rand.Next(data.MapSize))),
				Name = $"FeaturePoint{Position}",
				Texture = (Texture2D)GD.Load(TexturePath.GetFeaturePointTexturePath(data.TexturePackName)),
				Scale = new Vector2(0.1f, 0.1f)
			};

			if (!featurePoints.Contains(featurePoint))
			{
				featurePoints.Add(featurePoint);
			}
		}

		// 收敛
		blockBlones = Convergence(featurePoints);

		// 根据特征点清单构建点集
		foreach (Sprite2D sprite2D in featurePoints)
		{
			vectors.Add(new Vector2(sprite2D.Position.X, sprite2D.Position.Y));
		}

		// 德劳内三角剖分输出点集
		Point startPoint = DelaunayTriangle.Main(vectors, (int)CoordinateConverter.ToRealPosition(new Vector2(data.MapSize, data.MapSize)).X);

		AllocateColor(startPoint);

		// 异步上色
		await Task.Run(() =>
		{
			CallDeferred(nameof(UpdateColor), ColorDic, blockBlones);
		});

		// 统计
		blockNumber = CountBlockNumbers(blockBlones);
	}

	/// <summary>
	/// 创建一个方块并添加到地图中。
	/// </summary>
	/// <param name="blockPosition">方块的位置。</param>
	private void CreateBlock(Vector2 blockPosition)
	{
		Block block = ((PackedScene)GD.Load(PrefebPath.BlockPath)).Instantiate<Block>();

		block.Position = CoordinateConverter.ToRealPosition(blockPosition);
		block.Name = $"{blockPosition}";
		this.AddChild(block);
	}

	/// <summary>
	/// 通过特征点收敛方块。
	/// </summary>
	/// <param name="featurePoints">特征点列表。</param>
	/// <returns>每个方块对应的特征点字典。</returns>
	private Dictionary<Block, Sprite2D> Convergence(SpriteList featurePoints)
	{
		bool isConvergenced = false;
		Dictionary<Block, Sprite2D> blockBlones = new Dictionary<Block, Sprite2D>();

		while (!isConvergenced)
		{
			blockBlones = new Dictionary<Block, Sprite2D>();

			foreach (Node2D block in GetChildren())
			{
				if (block is Block)
				{
					PickFeaturePoint(featurePoints, (Block)block, blockBlones);
				}
			}

			isConvergenced = featurePoints.All(featurePoint => featurePoint.Position == GetCenterPosition(blockBlones, featurePoint));

			if (!isConvergenced)
			{
				foreach (Sprite2D featurePoint in featurePoints)
				{
					featurePoint.Position = GetCenterPosition(blockBlones, featurePoint);
				}
			}
		}
		return blockBlones;
	}

	/// <summary>
	/// 为每个方块选择最近的特征点。
	/// </summary>
	/// <param name="featurePoints">特征点列表。</param>
	/// <param name="block">方块。</param>
	/// <param name="bloneDic">方块对应的特征点字典。</param>
	private void PickFeaturePoint(SpriteList featurePoints, Block block, Dictionary<Block, Sprite2D> bloneDic)
	{
		var featureBlone = featurePoints.OrderBy(featurePoint => block.Position.DistanceTo(featurePoint.Position)).First();

		if (this.HasNode($"Line{block.Name}"))
		{
			GetNode<Line2D>($"Line{block.Name}").RemovePoint(1);
			GetNode<Line2D>($"Line{block.Name}").AddPoint(featureBlone.Position);
		}
		else
		{
			Line2D line = new Line2D()
			{
				Name = $"Line{block.Name}",
				Points = new Vector2[]{
					block.Position,
					featureBlone.Position
				},
				DefaultColor = new Color(1, 0, 0),
				Width = 1f,
			};
			this.AddChild(line);
		}
		UpdateDic<Block, Sprite2D>(block, featureBlone, bloneDic);
	}

	/// <summary>
	/// 更新字典中的键值对。
	/// </summary>
	/// <typeparam name="T">键的类型。</typeparam>
	/// <typeparam name="U">值的类型。</typeparam>
	/// <param name="key">键。</param>
	/// <param name="value">值。</param>
	/// <param name="dic">字典。</param>
	private void UpdateDic<[MustBeVariant] T, [MustBeVariant] U>(T key, U value, Dictionary<T, U> dic)
	{
		dic[key] = value; // 简化字典更新逻辑
	}

	/// <summary>
	/// 更新地图中每个方块的颜色。
	/// </summary>
	/// <param name="colorDic">点和颜色的对应关系字典。</param>
	/// <param name="blockBlones">每个方块对应的特征点字典。</param>
	public void UpdateColor(Dictionary<Vector2, Color> colorDic, Dictionary<Block, Sprite2D> blockBlones)
	{
		foreach (Node node in this.GetChildren())
		{
			if (node is Block)
			{
				Block block = (Block)node;
				if (blockBlones.TryGetValue(block, out Sprite2D featurePoint) && colorDic.TryGetValue(featurePoint.Position, out Color color))
				{
					block.Modulate = color;
				}
			}

			if (node is Line2D)
			{
				this.RemoveChild(node);
			}
		}
	}

	/// <summary>
	/// 获取特征点的中心位置。
	/// </summary>
	/// <param name="dictionary">方块对应的特征点字典。</param>
	/// <param name="featurePoint">特征点。</param>
	/// <returns>特征点的中心位置。</returns>
	private Vector2 GetCenterPosition(Dictionary<Block, Sprite2D> dictionary, Sprite2D featurePoint)
	{
		float sumX = 0;
		float sumY = 0;
		int count = 0;

		foreach (Pair pair in dictionary)
		{
			if (pair.Value == featurePoint)
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

	public PointList FinishPoints= new PointList();
	private void AllocateColor(Point startPoint)
	{
		// 检查邻居的逻辑是否正确
		FinishPoints.Add(startPoint);

		foreach (Point neighbor in startPoint.Neighbors)
		{
			this.AddChild(new Line2D(){ Points = [startPoint.Position, neighbor.Position] });
		}

		Color aimColor = new Color();
		foreach (Color color in ColorPool)
		{
			bool isRepeat = false;
			foreach (Point neighbor in startPoint.Neighbors)
			{
				if (ColorDic.ContainsKey(neighbor.Position) && ColorDic[neighbor.Position].Equals(color))
				{
					isRepeat = true;
					break;
				}
			}

			if (!isRepeat)
			{
				aimColor = color;
				break;
			}
		}

		if (aimColor == default)
		{
			aimColor = new Color(1f, 1f, 1f, 1f);
		}

		// 尝试上色
		ColorDic.Add(startPoint.Position, aimColor);

		foreach (Point neighbor in startPoint.Neighbors)
		{
			if (!FinishPoints.Contains(neighbor))
			{
				AllocateColor(neighbor);
			}
		}

	}

	/// <summary>
	/// 统计每个特征点对应的方块数量。
	/// </summary>
	/// <param name="blockBlones">每个方块对应的特征点字典。</param>
	/// <returns>每个特征点对应的方块数量字典。</returns>
	private Dictionary<Sprite2D, int> CountBlockNumbers(Dictionary<Block, Sprite2D> blockBlones)
	{
		Dictionary<Sprite2D, int> blockNumber = new Dictionary<Sprite2D, int>();

		foreach (Pair pair in blockBlones)
		{
			if (blockNumber.ContainsKey(pair.Value))
			{
				blockNumber[pair.Value]++;
			}
			else
			{
				blockNumber[pair.Value] = 1;
			}
		}

		return blockNumber;
	}
}