using Godot;
using System;

public partial class CameraInBasePlanetCreatingMenu : Camera2D
{
	public override void _Ready()
	{
		CameraInitializer();
	}

	public override void _Process(double delta)
	{
	}

	public override void _Input(InputEvent @event)
	{
		Vector2 _lastMousePos;
		// 鼠标中键拖动相机
		if (Input.IsMouseButtonPressed(MouseButton.Middle) && @event is InputEventMouseMotion eventMouseMotion)
		{
			var mouseDelta = eventMouseMotion.Relative / Zoom;

			// 平移相机
			Translate(new Vector2(-mouseDelta.X, -mouseDelta.Y));

			_lastMousePos = eventMouseMotion.Position / Zoom;
		}

		// 滚轮缩放
		if (Input.IsMouseButtonPressed(MouseButton.WheelUp) && this.Zoom > new Vector2(0.1f, 0.1f))
		{
			this.Zoom -= new Vector2(0.1f, 0.1f);
		}
		else if (Input.IsMouseButtonPressed(MouseButton.WheelDown))
		{
			this.Zoom += new Vector2(0.1f, 0.1f);
		}

		if (Input.IsKeyPressed(Key.R))
		{
			CameraInitializer();
		}
	}

	public void CameraInitializer()
	{
		this.Zoom = new Vector2(0.1f, 0.1f);
		this.Position = new Vector2(0, 0);
	}
}
