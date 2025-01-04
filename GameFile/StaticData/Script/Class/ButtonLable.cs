using Godot;
using System;

public partial class ButtonLable : Label
{
	public TextureButton Exit;
	
	// 这里定义不同情况下此Lable的坐标,包括Hover和Normal和Predssed状态
	private Vector2 NormalPosition = new Vector2(0, 0);
	private Vector2 HoverPosition = new Vector2(0, 2);
	private Vector2 PredssedPosition = new Vector2(0, 5);

	public override void _Ready()
	{
		// 获取父节点
		Exit = GetParent<TextureButton>();
	}

	public override void _Process(double delta)
	{
		// 设定不同情况下Lable的坐标(我把他们攒在一起是因为没什么好读的)
		if (Exit.ButtonPressed) Position = PredssedPosition;
		else if (Exit.IsHovered()) Position = HoverPosition;
		else Position = NormalPosition;
	}
}
