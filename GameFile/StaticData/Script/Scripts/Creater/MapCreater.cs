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
using SpriteList = System.Collections.Generic.List<Godot.Node2D>;
using VectorList = System.Collections.Generic.List<Godot.Vector2>;
using PointList = System.Collections.Generic.List<Point>;
using ColorList = System.Collections.Generic.HashSet<Godot.Color>;
using TaskList = System.Collections.Generic.List<System.Threading.Tasks.Task>;
using Pair = System.Collections.Generic.KeyValuePair<Godot.Vector2I, Godot.Vector2>;

public partial class MapCreater : Node2D
{
	private Data data;

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

	public void Main()
	{
		TileMapLayer map = new TileMapLayer() { TileSet = data.TexturePack }; // 地图层
		
		// 生成地基
		AddChild(map);
		CreateBaseMap(map);

		VectorList featurePoints = new VectorList(); // 特征点列表

		// 选定特征点
		GenerateFeaturePoints(featurePoints);

		Dictionary<Vector2I, Vector2> cellBlones = new Dictionary<Vector2I, Vector2>(); // <单元格:特征点>

		// 收敛
		cellBlones = Convergence(featurePoints, cellBlones, map);

		Dictionary<Vector2, int> cellNumber = CountCells(cellBlones);          			  // 储存每个区块的方格数量 
		Dictionary<Vector2, bool> oceanStatus = new Dictionary<Vector2, bool>();          // 储存每个特征点的海陆状态

		AllocateOcean(map, cellNumber, featurePoints, cellBlones);

		// 贪心算法分配海陆区块

		// // 板块移动
		// MovePlate(featurePoints, cellBlones, cellNumber, oceanStatus);
	}

	/// <summary>
	/// 用传入的<paramref name="Map"/>创建一个矩阵地图
	/// </summary>
	/// <param name="Map">地图层</param>
	private void CreateBaseMap(TileMapLayer Map)
	{
		for (int x = 0; x < data.MapSize; x++)
			for (int y = 0; y < data.MapSize; y++)
				Map.SetCell(new Vector2I(x, y), 0, new Vector2I(0, 0));
	}

	/// <summary>
	/// <para>为传入的<paramref name="featurePoints"/>生成随机的特征点</para>
	/// <para>随机数的范围是地图的边长, 而数量则是规定的板块数量</para>
	/// </summary>
	/// <param name="featurePoints">特征点容器</param>
	private void GenerateFeaturePoints(VectorList featurePoints)
	{
		while (featurePoints.Count < data.NumberOfPlates)
		{
			Random rand = new Random();

			Vector2 featurePoint = new Vector2(rand.Next(0, data.MapSize), rand.Next(0, data.MapSize));

			if (!featurePoints.Contains(featurePoint))
				featurePoints.Add(featurePoint);
		}
	}

	/// <summary>
	/// 使用K-means算法，将特征点聚类为板块
	/// </summary>
	/// <param name="featurePoints">特征点</param>
	/// <param name="cellBlones">单元格的归属数据(板块)</param>
	private Dictionary<Vector2I, Vector2> Convergence(VectorList featurePoints, Dictionary<Vector2I, Vector2> cellBlones, TileMapLayer map)
	{
		bool isConvergenced = false;

		while (!isConvergenced)
		{
			cellBlones = new Dictionary<Vector2I, Vector2>();

			// 选择距离最近的特征点作为归属
			foreach (Vector2I cell in map.GetUsedCells())
				ChooseFeaturePoint(cell, featurePoints, cellBlones);

			// 检查所有特征点是否收敛
			isConvergenced = featurePoints.All(featurePoint => featurePoint == GetCenterPosition(cellBlones, featurePoint));

			if (!isConvergenced)
	            for (int i = 0; i < featurePoints.Count; i++)
					// 更新特征点位置为对应方块的中心位置
    	            featurePoints[i] = GetCenterPosition(cellBlones, featurePoints[i]);
		}

		return cellBlones;
	}

	/// <summary>
	/// 为单元格在特征点上选择最近的点然后更新归属信息
	/// </summary>
	/// <param name="featurePoints">特征点</param>
	/// <param name="cell"></param>
	/// <param name="bloneDic"></param>
	private void ChooseFeaturePoint(Vector2I cell, VectorList featurePoints, Dictionary<Vector2I, Vector2> cellBlones)
	{
		// 用OrderBy()按照距离排序, 然后取第一个
		Vector2 featurePoint = featurePoints.OrderBy(fp => fp.DistanceTo(cell)).First();

		// 更新信息
		if (cellBlones.ContainsKey(cell))
			cellBlones[cell] = featurePoint;
		else
			cellBlones.Add(cell, featurePoint);
	}

	/// <summary>
	/// 获取传入的特征点在点集的中心位置
	/// </summary>
	/// <param name="cellBlones"></param>
	/// <param name="featurePoint"></param>
	/// <returns></returns>
	private Vector2 GetCenterPosition(Dictionary<Vector2I, Vector2> cellBlones, Vector2 featurePoint)
	{
		Vector2 sum = new Vector2(0, 0);
		int count = 0;

		// 遍历板块的所有单元格
		// 获取单元格数量以及XY坐标的和
		cellBlones.Where(pair => pair.Value == featurePoint).ToList().ForEach(pair =>
		{
			sum += pair.Key;
			count++;
		});

		float averageX = sum.X / count;
		float averageY = sum.Y / count;

		return new Vector2(averageX, averageY);
	}

	/// <summary>
	/// 统计每个特征点的单元格数量并返回
	/// </summary>
	/// <param name="cellBlones"></param>
	/// <returns></returns>
	private Dictionary<Vector2, int> CountCells(Dictionary<Vector2I, Vector2> cellBlones)
	{
		
		Dictionary<Vector2, int> cellNumber = new Dictionary<Vector2, int>();

		foreach (Pair pair in cellBlones)
			if (cellNumber.ContainsKey(pair.Value))
				cellNumber[pair.Value]++;
			else
				cellNumber.Add(pair.Value, 1);

		return cellNumber;
	}

	private void AllocateOcean(TileMapLayer map,  Dictionary<Vector2, int> cellNumber, VectorList featurePoints, Dictionary<Vector2I, Vector2> cellBlones)
	{
		int aimNum = (int)(cellNumber.Values.Sum() * (data.OceanToLandRatio / 100));
		int count = 0;

		foreach (Vector2 feature in featurePoints)
		{
			if (count < aimNum)
			{
				foreach (Vector2I cell in cellBlones.Keys.Where(cell => cellBlones[cell] == feature))
					ToOcean(map, cell);
			
				count += cellNumber[feature];
			}
			else
			{
				foreach (Vector2I cell in cellBlones.Keys.Where(cell => cellBlones[cell] == feature))
					ToLand(map, cell);
			}
		}

	}

	private void ToLand(TileMapLayer map, Vector2I cell)
	{
		// FIXME: 这里没有好好规定图源
		map.SetCell(cell, 3, new Vector2I(0, 0));
	}

	private void ToOcean(TileMapLayer map, Vector2I cell)
	{
		// FIXME: 这里没有好好规定图源
		map.SetCell(cell, 5, new Vector2I(0, 0));
	}

/*
	public void MovePlate(SpriteList featurePoints, Dictionary<Vector2I, Node2D> cellBlones, Dictionary<Node2D, int> cellNumber, Dictionary<Node2D, bool> oceanStatus)
	{
		// 构建板块移动方向
		Dictionary<Node2D, Vector2> moveDirections = new Dictionary<Node2D, Vector2>();
		Dictionary<Vector2I, Topography> topographyDic = new Dictionary<Vector2I, Topography>();
		Dictionary<Vector2I, int> cellHeight = new Dictionary<Vector2I, int>();

		// 初始化高度
		foreach (Pair pair in cellBlones)
		{
			cellHeight.Add(pair.Key, 0);
		}

		// 给板块随机的移动方向
		Random rand = new Random();
		foreach (Node2D featurePoint in featurePoints)
		{
			float angle = (float)(rand.NextDouble() * 2 * Math.PI); // 随机角度
			Vector2 direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
			moveDirections.Add(featurePoint, direction);
		}

		// 针对每个Vector2I检测
		foreach (Vector2I node in GetChildren().OfType<Vector2I>)
		{
			Vector2I cell = (Vector2I)node;
			Vector2I neighborVector2I = null;
			bool isBoundary = false;

			// 检测四向方块
			for (int i = -1; i < 1; i++)
				for (int j = -1; j < 1; j++)
				{
					Pair nearPair = cellBlones.First(x => x.Key.Position == cell.Position + new Vector2(i, j));

					// 排除自己以及四角方块
					if (Math.Abs(i) + Math.Abs(j) == 1 && nearPair.Key != cell)
					{
						// 判断是否为边界
						if (cellBlones[cell] != nearPair.Value)
						{
							isBoundary = true;
							neighborVector2I = nearPair.Key;
						}
					}
				}

			if (isBoundary)
			{
				// 候选结果: 山脉,火山,裂谷,洋中脊
				// 将自己的板块与对面板块的比较
				int cellState = oceanStatus[cellBlones[cell]] ? 0 : 1;
				int neighborVector2IState = oceanStatus[cellBlones[neighborVector2I]] ? 0 : 1;
				// 碰撞返回➕,否则返回➖
				int moveState = isCollision(cellBlones, moveDirections, cell, neighborVector2I) ? cellState + neighborVector2IState : cellState - neighborVector2IState;

				switch (moveState)
				{
					case 0:
						if (cellState == 1)
							SetRiftValley(cell); // 调用裂谷设置方法
						else
							SetMidOceanRidge(cell); // 调用洋中脊设置方法
						break;
					case 1:
						SetVolcano(cell); // 调用火山设置方法
						break;
					case 2:
						SetMountainRange(cell, topographyDic, cellHeight); // 调用山脉设置方法
						break;
					default:
						GD.Print("结果错误");
						break;
				}
			}
			else
			{
				SetIslandArc(cell); // 调用岛弧设置方法
			}
			// 放置盆地
		}
	}

	private void SetMountainRange(Vector2I cell, Dictionary<Vector2I, Topography> topographyDic, Dictionary<Vector2I, int> cellHeight)
	{
		// 山脉的设置逻辑
		// 更新字典
		// 放置山的icon

	}

	private void SetVolcano(Vector2I cell)
	{
		// 火山的设置逻辑
	}

	private void SetRiftValley(Vector2I cell)
	{
		// 裂谷的设置逻辑
	}

	private void SetMidOceanRidge(Vector2I cell)
	{
		// 洋中脊的设置逻辑
	}

	private void SetIslandArc(Vector2I cell)
	{
		// 岛弧的设置逻辑
	}

	private void SetBasin(Vector2I cell)
	{
		// 盆地的设置逻辑
	}

	private void SetAbyssalPlain(Vector2I cell)
	{
		// 海盆的设置逻辑
	}

	private bool isCollision(Dictionary<Vector2I, Node2D> cellBlones, Dictionary<Node2D, Vector2> moveDirections, Vector2I A, Vector2I B)
	{
		// 获取A和B的移动方向向量
		Vector2 directionA = moveDirections[cellBlones[A]];
		Vector2 directionB = moveDirections[cellBlones[B]];

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
	}*/

}