using Godot;
using System;

public partial class 这里放你的按钮名字 : Label
{
	//
	// 使用说明：
	// 每次开始时把脚本中的内容按照格式复制到你的按钮的组件中
	// 然后把类名替换成你的按钮的名字
	//
	// 从这里开始复制
	public TextureButton ExitButton;
	// 这里定义不同情况下此Lable的坐标,包括Hover和Normal和Predssed状态
	private Vector2 NormalPosition = new Vector2(0, 0);
	private Vector2 HoverPosition = new Vector2(0, 2);
	private Vector2 PredssedPosition = new Vector2(0, 5);

	public override void _Ready()
	{
		// 获取父节点
		ExitButton = GetParent<TextureButton>();
	}

	public override void _Process(double delta)
	{
		// 设定不同情况下Lable的坐标(我把他们攒在一起是因为没什么好读的)
		if (ExitButton.ButtonPressed) Position = PredssedPosition;
		else if (ExitButton.IsHovered()) Position = HoverPosition;
		else Position = NormalPosition;
	}
	// 从这里结束复制
}
