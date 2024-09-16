using Godot;
using System;
using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

public partial class 视差地图相机专用脚本 : Camera2D
{
	public int targetCamerHight; // 相机目标高度
	public int originalCamerHight = 0; // 相机原始高度
	public float cameraHight; // 相机真实高度
	public float intStep; // 相机高度变化步长
	private Node2D 地图生成器;

	private int cd;
	private int HeightRange = 4;
	Label 相机高度显示器;
	Label 各项参数监控;


	public override void _Ready()
	{
		targetCamerHight = originalCamerHight;
		cameraHight = originalCamerHight;
		地图生成器 = GetParent().GetNode<Node2D>("地图生成器");
		ChangeMapScale(cameraHight, 地图生成器, 1);
		ChangeMapOffset(this.Position, cameraHight);
		相机高度显示器 = GetParent().GetNode<Label>("相机高度");
		各项参数监控 = GetParent().GetNode<Label>("参数监控");
	}

	public override void _Process(double delta)
	{
		if (Math.Abs(targetCamerHight - cameraHight) < 0.1 && intStep != 0)
		{
			GD.Print($"插值完成, cameraheight = {cameraHight}");
			originalCamerHight = targetCamerHight;
			cameraHight = targetCamerHight;
			intStep = 0;

			// 这里应该最后再规整一下每一层,也就是收个尾,将所有层直接放到目标高度
			ChangeMapScale(targetCamerHight, 地图生成器);
			ChangeMapOffset(this.Position, targetCamerHight);

			相机高度显示器.Text = $"相机高度: {cameraHight}\n目标高度: {targetCamerHight}\n步长: {intStep}";
		}
		else if (cameraHight != targetCamerHight)
		{
			GD.Print($"插值将cameraheight靠近目标height:{intStep}");
			// 插值将cameraheight靠近目标height
			cameraHight += intStep;

			// 缩放地图
			ChangeMapScale(cameraHight, 地图生成器);
			ChangeMapOffset(this.Position, cameraHight);

			相机高度显示器.Text = $"相机高度: {cameraHight}\n目标高度: {targetCamerHight}\n步长: {intStep}";
		}

		if (cd != 0)
		{
			cd = 0;
		}
	}

	// 输入事件统一放在這
	// 升降相机,当滚轮滚动时, 对应加减camerHight
	public override void _Input(InputEvent @event)
	{
		// 处理滚轮事件
		// 我开放的
		if (@event is InputEventMouseButton mouseButton && cd == 0)
		{
			// Up -> +Level
			if (mouseButton.ButtonIndex == MouseButton.WheelUp && targetCamerHight < 500)
			{
				targetCamerHight++;
			}
			// Down -> -Level
			if (mouseButton.ButtonIndex == MouseButton.WheelDown && targetCamerHight > 0)
			{
				targetCamerHight--;
			}
			cd = 1;

			相机高度显示器.Text = $"相机高度: {cameraHight}\n目标高度: {targetCamerHight}\n步长: {intStep}";

			// 计算步长
			intStep = (targetCamerHight - cameraHight) / (int)(Engine.GetFramesPerSecond() * 0.36); // 步长 = 要移动的距离 / 移动的时间

			ChangeMapScale(cameraHight, 地图生成器); // 调用函数调整地图缩放
			ChangeMapOffset(this.Position, cameraHight); // 调用函数调整地图偏移
		}

		Vector2 _lastMousePos;
		// 鼠标中键拖动相机
		if (Input.IsMouseButtonPressed(MouseButton.Middle))
		{
			if (@event is InputEventMouseMotion eventMouseMotion)
			{
				if (Input.IsMouseButtonPressed(MouseButton.Middle))
				{
					var mouseDelta = eventMouseMotion.Relative;

					Translate(new Vector2(-mouseDelta.X, -mouseDelta.Y));

					// 更新地图偏移量
					ChangeMapOffset(this.Position, cameraHight);
				}
				_lastMousePos = eventMouseMotion.Position;
			}
		}
	}


	// 改变地图缩放的函数
	// 大于基准高度的不显示，小于基准高度的缩小
	public void ChangeMapScale(float baseHeight, Node2D 地图生成器,int Mode = 0)
	{
		int beginHeight;
		int endHeight;
		
		if (Mode == 0)
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
			{
				Level = 地图生成器.GetChild<Node2D>(i);
			}
			else 
			{
				break;
			}
			
			// 缩放
			float LevelDiff = baseHeight - float.Parse(Level.Name);
			float ScaleValue = (float)Math.Pow(0.5, LevelDiff); // 更改此处的值来调整缩放大小
			Level.Scale = new Vector2(ScaleValue, ScaleValue); //进行缩放
			
			// 透明度
			if (LevelDiff < 0 && LevelDiff > -1)
			{
				Level.Modulate = new Color(Level.Modulate.R, Level.Modulate.G, Level.Modulate.B, 1 - Math.Abs(LevelDiff));
			}
			else if (LevelDiff <= -1 || LevelDiff >= HeightRange)
			{
				Level.Modulate = new Color(Level.Modulate.R, Level.Modulate.G, Level.Modulate.B, 0);
			}
			else
			{
				Level.Modulate = new Color(Level.Modulate.R, Level.Modulate.G, Level.Modulate.B, 1);
			}

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

	// 改变地图偏移量
	// FIXME: 偏移函数需要重新计算参数，高度差大于两层的地图出现了偏移过快的问题
	// FIXME: 这里其实可以优化,每次不遍历所有的层,只需要遍历当前相机高度附近的层
	public void ChangeMapOffset(Vector2 CameraPosition, float baseHeight)
	{
		// GD.Print("------------------------\n");

		// 笔记: 范围为 [-1, HeightRange]
		// 
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
			{
				Level = 地图生成器.GetChild<Node2D>(i);
			}
			else 
			{
				break;
			}

			Level.Position = new Vector2(CameraPosition.X * -(Level.Scale.X - 1), CameraPosition.Y * -(Level.Scale.Y - 1)).Round(); // L * 坐标值 / 缩放值
			// GD.Print($"\nLevelName{Level.Name}\nlScale{Level.Scale}\nPosition{Level.Position}");
			// GD.Print($"算式:\n\t{Level.Position.X}(X) = 64 (L) * {CameraPosition.X} (摄像机X坐标) * {Level.Scale.X} (缩放值)\n");
		}

		// GD.Print("\n------------------------");

	}
}

