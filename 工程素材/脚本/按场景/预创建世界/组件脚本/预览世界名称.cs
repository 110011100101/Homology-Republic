using Godot;
using System;

public partial class 预览世界名称 : Label
{
	// 预设位置坐标
	public Vector2 prePosition = new Vector2(0, 60);

	public override void _Ready()
	{	
		OnTextChange("世界名称");
	}

	public override void _Process(double delta)
	{
	}

	// 更新
	public void OnTextChange(string new_text)
	{
		// 更新文字
		Text = new_text;
	}
}
