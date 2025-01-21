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
using TaskList = System.Collections.Generic.List<System.Threading.Tasks.Task>;
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
		Dictionary<Sprite2D, bool> oceanStatus = new Dictionary<Sprite2D, bool>();      // 储存每个特征点的海陆状态

		// 生成地基
		await CreateBaseMap();

		// 选定特征点
		GenerateFeaturePoints(featurePoints, rand);

		// 收敛
		blockBlones = Convergence(featurePoints);

		// 根据特征点清单构建点集
		vectors.AddRange(featurePoints.Select(x => new Vector2(x.Position.X, x.Position.Y)));

		// 德劳内三角剖分输出点集
		Point startPoint = DelaunayTriangle.Main(vectors, (int)CoordinateConverter.ToRealPosition(new Vector2(data.MapSize, data.MapSize)).X);

		// 颜色分配
		AllocateColor(startPoint, new PointList());

		// 异步上色
		await Task.Run(() => CallDeferred(nameof(UpdateColor), ColorDic, blockBlones) );

		// 统计每个特征点对应的方块数量
		blockNumber = CountBlockNumbers(blockBlones);

		// 贪心算法分配海陆区块
		AllocateOcean(featurePoints, blockNumber, oceanStatus, blockBlones);

		// 板块移动
		
	}

	private async Task CreateBaseMap()
	{
		TaskList tasks= new TaskList(); 

		for (int i = 0; i < data.MapSize; i++)
		{
			tasks.Add(Task.Run(() =>
			{
				Enumerable.Range(0, data.MapSize).ToList().ForEach(j => CallDeferred(nameof(CreateBlock), new Vector2(i, j)));
			}));
			await Task.Delay(1);
		}
		await Task.WhenAll(tasks);
	}

	private void CreateBlock(Vector2 blockPosition)
	{
		Block block = ((PackedScene)GD.Load(PrefebPath.BlockPath)).Instantiate<Block>();

		block.Position = CoordinateConverter.ToRealPosition(blockPosition);
		block.Name = $"{blockPosition}";
		this.AddChild(block);
	}

	private void GenerateFeaturePoints(SpriteList featurePoints, Random rand)
	{
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
	}

	private Dictionary<Block, Sprite2D> Convergence(SpriteList featurePoints)
	{
		bool isConvergenced = false;
		Dictionary<Block, Sprite2D> blockBlones = new Dictionary<Block, Sprite2D>();

		// 迭代收敛算法，直到每个特征点的位置不再发生变化
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

			// 检查所有特征点是否收敛
			isConvergenced = featurePoints.All(featurePoint => featurePoint.Position == GetCenterPosition(blockBlones, featurePoint));

			if (!isConvergenced)
			{
				// 更新特征点位置为对应方块的中心位置
				featurePoints.ForEach(featurePoint => featurePoint.Position = GetCenterPosition(blockBlones, featurePoint));
			}
		}
		return blockBlones;
	}

	private void PickFeaturePoint(SpriteList featurePoints, Block block, Dictionary<Block, Sprite2D> bloneDic)
	{
		Sprite2D featureBlone = featurePoints.OrderBy(featurePoint => block.Position.DistanceTo(featurePoint.Position)).First();

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

	private void UpdateDic<[MustBeVariant] T, [MustBeVariant] U>(T key, U value, Dictionary<T, U> dic)
	{
		dic[key] = value; // 简化字典更新逻辑
	}

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

	private Vector2 GetCenterPosition(Dictionary<Block, Sprite2D> dictionary, Sprite2D featurePoint)
	{
		float sumX = 0;
		float sumY = 0;
		int count = 0;

		dictionary.Where(pair => pair.Value == featurePoint).ToList().ForEach(pair =>
		{
			sumX += pair.Key.Position.X;
			sumY += pair.Key.Position.Y;
			count++;
		});

		float averageX = sumX / count;
		float averageY = sumY / count;

		return new Vector2(averageX, averageY);
	}

	private void AllocateColor(Point startPoint, PointList finishPoints)
	{
		// 检查邻居的逻辑是否正确
		finishPoints.Add(startPoint);

		// 绘制邻居连接线
		foreach (Point neighbor in startPoint.Neighbors)
		{
			this.AddChild(new Line2D(){ Points = [startPoint.Position, neighbor.Position] });
		}

		// 为当前点分配颜色，确保邻居点颜色不同
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
			// 默认颜色为白色
			aimColor = new Color(1f, 1f, 1f, 1f);
		}

		// 尝试上色
		ColorDic.Add(startPoint.Position, aimColor);

		// 递归为邻居点分配颜色
		foreach (Point neighbor in startPoint.Neighbors)
		{
			if (!finishPoints.Contains(neighbor))
			{
				AllocateColor(neighbor, finishPoints);
			}
		}

	}

	private async void AllocateOcean(SpriteList featurePoints, Dictionary<Sprite2D, int> blockNumber, Dictionary<Sprite2D, bool> oceanStatus,  Dictionary<Block, Sprite2D> blockBlones)
	{
		// 尝试不同海陆组合并挑选出最接近海陆比的组合
		float targetOceanRatio = data.OceanToLandRatio; 
		int totalBlocks = blockNumber.Values.Sum();
		int targetOceanBlocks = (int)(totalBlocks * (targetOceanRatio / 100));

		// 使用贪心算法分配海陆区块
		SpriteList sortedFeaturePoints = featurePoints.OrderBy(fp => blockNumber[fp]).ToList();
		int currentOceanBlocks = 0;

		await Task.Delay(1000);

		foreach (Sprite2D featurePoint in sortedFeaturePoints)
		{
			TaskList tasks = new TaskList();
			if (currentOceanBlocks < targetOceanBlocks)
			{
				// 分配为海洋
				foreach (Pair pair in blockBlones)
				{
                    tasks.Add(Task.Run(() =>
                    {
                        if (pair.Value == featurePoint)
                        {
                            CallDeferred(nameof(ToOcean), pair.Key);
                            currentOceanBlocks++;

                        }
                    }));

					if (pair.Value == featurePoint)
					{
						// 延迟以展示变化过程
						await Task.Delay(1);
					}
				}
				oceanStatus[featurePoint] = true;
			}
			else
			{
				// 分配为陆地
				foreach (Pair pair in blockBlones)
				{
					tasks.Add(Task.Run(() =>
                    {
						if (pair.Value == featurePoint)
						{
							CallDeferred(nameof(ToLand), pair.Key);
						}
                    }));

					if (pair.Value == featurePoint)
					{
						// 延迟以展示变化过程
						await Task.Delay(1);
					}
				}
				oceanStatus[featurePoint] = false;
			}
			await Task.WhenAll(tasks);
		}
	}

	private void ToLand(Block block)
	{
		
		block.ChangeGroundMaterial(new earth(data.TexturePackName));
		block.Modulate = new Color(1.0f, 1.0f, 1.0f, 1.0f);
	}

	private void ToOcean(Block block)
	{
		block.ChangeGroundMaterial(new water(data.TexturePackName));
		block.Modulate = new Color(1.0f, 1.0f, 1.0f, 1.0f);
	}

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

	public void MovePlate(Dictionary<Block, Sprite2D> blockBlones, Dictionary<Sprite2D, int> blockNumber, Dictionary<Sprite2D, bool> oceanStatus)
	{
		
	}

}