using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

public partial class MapCreater : Node2D
{
	private Data data;
	private Notice_BasePlanetCreatingMenu notice;
	private Godot.Collections.Dictionary<Block, Sprite2D> BlockBlones;
	
	// 颜色池
	private Array<Godot.Color> color_pool = new Array<Godot.Color>() { 
		new Godot.Color(1.0f, 0.498f, 0.314f, 1.0f),	// 橙红色
		new Godot.Color(0.392f, 0.584f, 0.929f, 1.0f),	// 天蓝色
		new Godot.Color(0.769f, 0.306f, 0.804f, 1.0f),	// 紫罗兰色
		new Godot.Color(0.984f, 0.604f, 0.604f, 1.0f),	// 桃红色
	};

	public async override void _Ready()
	{
		// init
		data = GetNode<Data>("/root/Data");
		notice = GetNode<Notice_BasePlanetCreatingMenu>("/root/大地图生成/Notice");
		GlobalPosition = new Vector2(0, -(CoordinateConverter.ToRealPosition(new Vector2(data.MapSize, data.MapSize)).Y / 2)); // 把 MapSize 转成真实坐标后取一半

		notice.OutputNotice("地图生成器准备完毕");
	}

	public override void _Process(double delta)
	{
	}

	public async void WorkFlow()
	{
		//
		// 划分板块
		// 
		// 生成地基
		notice.OutputNotice("开始生成地基");

		// 按行并行生成
		for (int i = 0; i < data.MapSize; i++)
		for (int j = 0; j < data.MapSize; j++)
			_CreateBlock(new Vector2(i, j));

		notice.OutputNotice("地基生成完毕");
		//
		// 放置特征点 
		//
		Random rand = new Random();
		HashSet<Vector2> FeatureSet = new HashSet<Vector2>();
		Array<Sprite2D> FeaturePoints = new Array<Sprite2D>();
		Godot.Collections.Dictionary<Sprite2D, Godot.Color> color_dic;


		notice.OutputNotice("开始放置特征点");

		// 选定特征点
		while (FeatureSet.Count < data.NumberOfPltes)
			FeatureSet.Add(new Vector2(rand.Next(data.MapSize), rand.Next(data.MapSize)));

		notice.OutputNotice("特征点放置完毕");


		notice.OutputNotice("开始收敛");

		FeaturePoints = _AddFeaturePoints(FeatureSet);
		_Convergence(FeaturePoints);

		notice.OutputNotice("收敛完毕");


		// 接下来需要导出特征点归属集合
		// 然后清理线条和特征点,并且给每个block分配大陆并提供按钮以显示颜色区分的视图
		// BlockBlones; // 这个是最终的特征点字典
		notice.OutputNotice("开始上色");
		
		FeaturePoints = _SortFeature(FeaturePoints); // 确保特征点是按距离排序的
		color_dic = _UpdateColor(color_pool, FeaturePoints); // 更新颜色分配

		foreach (Node temp in GetChildren())
		{
			await Task.Factory.StartNew(() => {

				CallDeferred("_ChangeColor", temp, FeaturePoints, color_dic);
			});
		}
		notice.OutputNotice("上色完成");

		// 把特征点按顺序用箭头连起来
		
		for (int i = 0; i < FeaturePoints.Count - 1; i++)
		{
			AddChild(new Label(){ Text = $"{i}", Scale = new Vector2(15, 15), Position = FeaturePoints[i].Position});
			Vector2 start = FeaturePoints[i].Position;
			Vector2 end = FeaturePoints[i + 1].Position;
			await Task.Factory.StartNew(() => {
				CallDeferred("_DrawLine", start, end);
				Task.Delay(100);
			});
		}

		// 
		// 选配地形
		// 
		// 海陆划分
		
	}

	private void _DrawLine(Vector2 start, Vector2 end)
	{
		Line2D line = new Line2D()
		{
			Points = new Vector2[]{ start, end },
			DefaultColor = new Godot.Color(0, 0, 0),
			Width = 5f,
		};
		this.AddChild(line);
	}
	
	private void _ChangeColor(Node temp, Array<Sprite2D> FeaturePoints, Godot.Collections.Dictionary<Sprite2D, Godot.Color> color_dic)
	{
		if (temp.GetType() == typeof(Line2D))
		{
			Line2D templine = (Line2D)temp;
			Block tempblock = GetNode<Block>($"{CoordinateConverter.ToMatrixPosition(templine.GetPointPosition(0))}");

			tempblock.Modulate = color_dic[BlockBlones[tempblock]]; // 修改颜色
			temp.QueueFree(); // 清除线条
		}
	}

	private Godot.Collections.Dictionary<Sprite2D, Godot.Color> _UpdateColor(Array<Godot.Color> color_pool, Array<Sprite2D> FeaturePoints)
	{
		Godot.Collections.Dictionary<Sprite2D, Godot.Color> color_dic = new Godot.Collections.Dictionary<Sprite2D, Godot.Color>();

		int pool_count = 0;

		foreach (Sprite2D feature in FeaturePoints)
		{
			color_dic.Add(feature, color_pool[pool_count]);

			if (pool_count < color_pool.Count - 1)
			{
				pool_count++;
			}
			else
			{
				pool_count = 0;
			}
		}
		return color_dic;
	}

	private void _CreateBlock(Vector2 BlockPosition)
	{
		// init
		Block block = ((PackedScene)GD.Load("res://工程素材/脚本/预制体/Block/Block.tscn")).Instantiate<Block>();

		block.Position = CoordinateConverter.ToRealPosition(BlockPosition);
		block.Name = $"{BlockPosition}";
		this.AddChild(block);
	}

	private Array<Sprite2D> _AddFeaturePoints(HashSet<Vector2> FeatureSet)
	{
		int n = 0;
		Array<Sprite2D> FeaturePoints = new Array<Sprite2D>();
		foreach (Vector2 position in FeatureSet)
		{
			Sprite2D FeaturePoint = new Sprite2D()
			{
				Position = CoordinateConverter.ToRealPosition(position),
				Name = $"FeaturePoint{n}",
				Texture = (Texture2D)GD.Load("res://工程素材/材质包/方格概念/tiles/FeaturePoints.png"),
			};
			FeaturePoints.Add(FeaturePoint);
			AddChild(FeaturePoint);

			n++;
		}
		return FeaturePoints;
	}

	// 收敛
	private void _Convergence(Array<Sprite2D> FeaturePoints)
	{
		bool isConvergenced = false;
		Godot.Collections.Dictionary<Block, Sprite2D> BloneDic = new Godot.Collections.Dictionary<Block, Sprite2D>();

		while (!isConvergenced)
		{
			BloneDic = new Godot.Collections.Dictionary<Block, Sprite2D>();

			foreach (Node2D block in GetChildren())
			{
				if (block.GetType() == typeof(Block))
				{
					_PickFeaturePoint(FeaturePoints, (Block)block, BloneDic);
				}
			}

			// 调整特征点然后决定是否继续遍历
			// 求出中心位置,然后把特征点放过去
			foreach (Sprite2D FeaturePoint in FeaturePoints)
			{
				if (FeaturePoint.Position != _CenterPosition(BloneDic, FeaturePoint))
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
					FeaturePoint.Position = _CenterPosition(BloneDic, FeaturePoint);
				}
			}
		}
		BlockBlones = BloneDic;
	}

	/// <summary>
	///	选择特征点
	///	<span>这个方法会将传入的 <paramref name="block"/> 与传入的 <paramref name="FeaturePoints"/> 集合进行轮询</span>
	/// </summary>
	/// <param name="FeaturePoints">特征点集</param>
	/// <param name="block">block</param>
	private void _PickFeaturePoint(Array<Sprite2D> FeaturePoints, Block block, Godot.Collections.Dictionary<Block, Sprite2D> BloneDic)
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
					DefaultColor = new Godot.Color(1, 0, 0),
					Width = 1f,
				};
				this.AddChild(AFuckingLine);
			}
		}
		_UpdateDic<Block, Sprite2D>(block, FeatureBlone, BloneDic);
	}

	private void _UpdateDic<[MustBeVariant] T, [MustBeVariant] U>(T key, U value, Godot.Collections.Dictionary<T, U> dic)
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

	private Vector2 _CenterPosition(Godot.Collections.Dictionary<Block, Sprite2D> dictionary, Sprite2D FeaturePoint)
	{
		float sumX = 0;
		float sumY = 0;
		int count = 0;

		foreach (KeyValuePair<Block, Sprite2D> pair in dictionary)
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


	private Array<Sprite2D> _SortFeature(Array<Sprite2D> FeaturePoints)
	{
		// 准备采用德劳内三角剖分
	}
}

