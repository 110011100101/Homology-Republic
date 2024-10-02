using Godot;
using System;
using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

public partial class 视差地图相机专用脚本 : Camera2D
{
	[Export] private int HeightRange = 4;
	[Export] private double scaleFactor = 0.5;

	public int targetCamerHight; // 相机目标高度
	public int originalCamerHight = 0; // 相机原始高度
	public float cameraHight; // 相机真实高度
	public float intStep; // 相机高度变化步长
	private Node2D 地图生成器;

	private int cd;
	Label 相机高度显示器;
	Label 各项参数监控;


	public override void _Ready()
	{
		// 初始化参数
		targetCamerHight = originalCamerHight;
		cameraHight = originalCamerHight;

		// 获取节点
		地图生成器 = GetParent().GetNode<Node2D>("地图生成器");
		相机高度显示器 = GetParent().GetNode<Label>("相机高度");
		各项参数监控 = GetParent().GetNode<Label>("参数监控");

		// 初始化地图
		ChangeMapOffset(this.Position, cameraHight);
		ChangeMapScale(cameraHight, 地图生成器, false);
	}

	public override void _Process(double delta)
	{
		if (Math.Abs(targetCamerHight - cameraHight) < 0.1 && intStep != 0)
		{
			// 初始化值
			originalCamerHight = targetCamerHight;
			cameraHight = targetCamerHight;
			intStep = 0;

			// 归拢层
			ChangeMapScale(targetCamerHight, 地图生成器);
			ChangeMapOffset(this.Position, targetCamerHight);

			相机高度显示器.Text = $"相机高度: {cameraHight}\n目标高度: {targetCamerHight}\n步长: {intStep}";
		}
		else if (cameraHight != targetCamerHight)
		{
			// 插值将cameraheight靠近目标height
			cameraHight += intStep;

			// 缩放地图
			ChangeMapScale(cameraHight, 地图生成器);
			ChangeMapOffset(this.Position, cameraHight);

			相机高度显示器.Text = $"相机高度: {cameraHight}\n目标高度: {targetCamerHight}\n步长: {intStep}";
		}

		if (cd != 0)
			cd = 0;
	}

	// 输入事件
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseButton && cd == 0)
		{
			// MouseUp -> Level up
			if (mouseButton.ButtonIndex == MouseButton.WheelUp && targetCamerHight < 500)
				targetCamerHight++;

			// MouseDown -> Level down
			if (mouseButton.ButtonIndex == MouseButton.WheelDown && targetCamerHight > 0)
				targetCamerHight--;

			cd = 1;

			// [监测]相机参数
			相机高度显示器.Text = $"相机高度: {cameraHight}\n目标高度: {targetCamerHight}\n步长: {intStep}";

			// 计算步长
			intStep = (targetCamerHight - cameraHight) / (int)(Engine.GetFramesPerSecond() * 0.36); // 步长 = 要移动的距离 / 移动的时间

			ChangeMapScale(cameraHight, 地图生成器); // 调用函数调整地图缩放
			ChangeMapOffset(this.Position, cameraHight); // 调用函数调整地图偏移
		}

		Vector2 _lastMousePos;
		// 鼠标中键拖动相机
		if (Input.IsMouseButtonPressed(MouseButton.Middle) && @event is InputEventMouseMotion eventMouseMotion)
		{
			var mouseDelta = eventMouseMotion.Relative;

			// 平移相机
			Translate(new Vector2(-mouseDelta.X, -mouseDelta.Y));

			// 更新地图偏移量
			ChangeMapOffset(this.Position, cameraHight);

			_lastMousePos = eventMouseMotion.Position;
		}
	}

	/// <summary>
	/// 地图缩放
	/// </summary>
	/// <param name="baseHeight">基准高度<para>
	/// 此高度基本上是相机的高度, 基准高度的意义在于, 通过改变基准高度像电梯一样切换层级
	/// </para></param>
	/// <param name="地图生成器">因为此脚本挂载到相机节点, 所以需要获取地图生成器来直接操作缩放, 不然又要用信号传来传去的贼麻烦还不好维护</param>
	/// <param name="Mode">此参数提供了两种模式
	/// <para>0: 默认模式, 会根据相机高度自动缩放</para>
	/// <para>1: 初始化模式, 会缩放所有层</para></param>
	public void ChangeMapScale(float baseHeight, Node2D 地图生成器, bool Mode = true)
	{
		int beginHeight;
		int endHeight;

		if (Mode)
		{
			if (baseHeight < HeightRange)
			{
				beginHeight = 0;
				endHeight = (int)baseHeight + 2;
			}
			else
			{
				beginHeight = (int)baseHeight - HeightRange;
				endHeight = (int)baseHeight + 2;
			}
		}
		else
		{
			beginHeight = 0;
			endHeight = 地图生成器.GetChildCount() - 1;
		}

		for (int i = beginHeight; i <= endHeight; i++)
		{
			Node2D Level;

			if (i < 地图生成器.GetChildCount())
				Level = 地图生成器.GetChild<Node2D>(i);
			else
				break;

			// 缩放
			float LevelDiff = baseHeight - float.Parse(Level.Name);
			float ScaleValue = (float)Math.Pow(scaleFactor, LevelDiff);
			Level.Scale = new Vector2(ScaleValue, ScaleValue); // 进行缩放

			// 透明度
			if (LevelDiff < 0 && LevelDiff > -1)
				Level.Modulate = new Color(Level.Modulate.R, Level.Modulate.G, Level.Modulate.B, 1 - Math.Abs(LevelDiff));
			else if (LevelDiff <= -1 || LevelDiff >= HeightRange)
				Level.Modulate = new Color(Level.Modulate.R, Level.Modulate.G, Level.Modulate.B, 0);
			else
				Level.Modulate = new Color(Level.Modulate.R, Level.Modulate.G, Level.Modulate.B, 1);

			// 是否可见
			if (LevelDiff <= -1 || LevelDiff >= HeightRange)
			{
				Level.Visible = false;
			}
			else
			{
				Level.Visible = true;
			}
		}
	}

	/// <summary>
	/// 地图偏移
	/// <para>
	/// <b>注意:</b> 适用范围为 [-1, HeightRange] 通常此范围会从当前的相机高度向上多算一个, 因此从 -1 开始.
	/// </para>
	/// <para>
	/// <b>HeightRange:</b> 定义了参与运算的层数, 也就是往下看能显示多少层, 一般头顶的层虽然不显示, 但是依然参与运算.
	/// </para>
	/// </summary>
	/// <param name="CameraPosition">相机位置</param>
	/// <param name="baseHeight">基准高度</param>
	public void ChangeMapOffset(Vector2 CameraPosition, float baseHeight)
	{
		int beginHeight;
		int endHeight;

		if (baseHeight < HeightRange)
		{
			beginHeight = 0;
			endHeight = (int)baseHeight + 2;
		}
		else
		{
			beginHeight = (int)baseHeight - HeightRange;
			endHeight = (int)baseHeight + 2;
		}

		for (int i = beginHeight; i <= endHeight; i++)
		{
			Node2D Level;

			if (i < 地图生成器.GetChildCount())
				Level = 地图生成器.GetChild<Node2D>(i);
			else
				break;

			Level.Position = new Vector2(CameraPosition.X * -(Level.Scale.X - 1), CameraPosition.Y * -(Level.Scale.Y - 1)).Round(); // L * 坐标值 / 缩放值
		}
	}
}

