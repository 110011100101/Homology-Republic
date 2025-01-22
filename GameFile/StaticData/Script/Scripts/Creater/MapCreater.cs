using Godot;
using Godot.Collections;
using RoseIsland.Library.Algorithm.DelaunayTriangle;
using RoseIsland.Library.CalculationTool.CoordinateConverter;
using RoseIsland.Library.CalculationTool.Determinant;
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

	public void MovePlate(SpriteList featurePoints, Dictionary<Block, Sprite2D> blockBlones, Dictionary<Sprite2D, int> blockNumber, Dictionary<Sprite2D, bool> oceanStatus)
	{
		// 构建板块移动方向
		Dictionary<Sprite2D, Vector2> moveDirections = new Dictionary<Sprite2D, Vector2>();
		Dictionary<Block, Topography> topographyDic = new Dictionary<Block, Topography>();
		Dictionary<Block, int> blockHeight = new Dictionary<Block, int>();

		// 初始化高度
		foreach (Pair pair in blockBlones)
		{
			blockHeight.Add(pair.Key, 0); 
		}

		// 给板块随机的移动方向
		Random rand = new Random();
		foreach (Sprite2D featurePoint in featurePoints)
		{
			float angle = (float)(rand.NextDouble() * 2 * Math.PI); // 随机角度
			Vector2 direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
			moveDirections.Add(featurePoint, direction);
		}

		// 针对每个Block检测
		foreach (Node2D node in GetChildren())
		{
			if (node is Block)
			{
				Block block = (Block)node;
				Block neighborBlock = null;
				bool isBoundary = false;
				
				// 检测四向方块
				for (int i = -1; i < 1; i++)
				for (int j = -1; j < 1; j++)
				{
					Pair nearPair = blockBlones.First(x => x.Key.Position == block.Position + new Vector2(i, j));

					// 排除自己以及四角方块
					if (Math.Abs(i) + Math.Abs(j) == 1 && nearPair.Key != block)
					{
						// 判断是否为边界
						if (blockBlones[block] != nearPair.Value)
						{
							isBoundary = true;
							neighborBlock = nearPair.Key;
						}
					}
				}

				if (isBoundary)
				{
					// 候选结果: 山脉,火山,裂谷,洋中脊
					// 将自己的板块与对面板块的比较
					int blockState = oceanStatus[blockBlones[block]] ? 0 : 1;
					int neighborBlockState = oceanStatus[blockBlones[neighborBlock]] ? 0 : 1;
					// 碰撞返回➕,否则返回➖
					int moveState =  isCollision(blockBlones, moveDirections, block, neighborBlock) ? blockState + neighborBlockState : blockState - neighborBlockState;

					switch (moveState)
					{
						case 0:
							if (blockState == 1)
								SetRiftValley(block); // 调用裂谷设置方法
							else
								SetMidOceanRidge(block); // 调用洋中脊设置方法
							break;
						case 1:
							SetVolcano(block); // 调用火山设置方法
							break;
						case 2:
							SetMountainRange(block, topographyDic, blockHeight); // 调用山脉设置方法
							break;
						default:
							GD.Print("结果错误");
							break;
					}
				}
				else
				{
					SetIslandArc(block); // 调用岛弧设置方法
				}
			}
				// 放置盆地
		}
	}

	private void SetMountainRange(Block block,Dictionary<Block, Topography> topographyDic, Dictionary<Block, int> blockHeight)
	{
		// 山脉的设置逻辑
		// 更新字典
		// 放置山的icon
		
	}

	private void SetVolcano(Block block)
	{
		// 火山的设置逻辑
	}

	private void SetRiftValley(Block block)
	{
		// 裂谷的设置逻辑
	}

	private void SetMidOceanRidge(Block block)
	{
		// 洋中脊的设置逻辑
	}

	private void SetIslandArc(Block block)
	{
		// 岛弧的设置逻辑
	}

	private void SetBasin(Block block)
	{
		// 盆地的设置逻辑
	}

	private void SetAbyssalPlain(Block block)
	{
		// 海盆的设置逻辑
	}

	private bool isCollision(Dictionary<Block, Sprite2D> blockBlones, Dictionary<Sprite2D, Vector2> moveDirections, Block A, Block B)
	{
		// 获取A和B的移动方向向量
		Vector2 directionA = moveDirections[blockBlones[A]];
		Vector2 directionB = moveDirections[blockBlones[B]];

		// 构建A旋转90°后的向量
		Vector2 rotatedA = new Vector2(-directionA.Y, directionA.X);

		// 将90A和B转写成行列式
		double[,] matrix = new double[2, 2];
		matrix[0, 0] = rotatedA.X;
		matrix[0, 1] = rotatedA.Y;
		matrix[1, 0] = directionB.X;
		matrix[1, 1] = directionB.Y;

		// 使用工具计算行列式的正负
		int determinantSign = Determinant.GetDeterminantSign(matrix);

		// 若是夹钝角且在此向量顺时针90°的向量左侧，则为碰撞
		if (!Determinant.IsAcuteAngle(directionA, directionB))
		{
			return determinantSign > 0 ? true : false;
		}
		return false;
	}
	
}