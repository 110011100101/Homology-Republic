using Godot;
using System;

public partial class 设置按钮文字 : Label
{
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
		// 设定不同情况下Lable的坐标(我把他们攒在一起是因为没什么好读的，不是因为我不知道什么是代码规范，也不是懒)
		if (ExitButton.ButtonPressed) Position = PredssedPosition;
		else if (ExitButton.IsHovered()) Position = HoverPosition;
		else Position = NormalPosition;
	}
	
	// 这里定义设置按钮的点击事件
	// 即打开设置界面
	// TODO: 这里的场景并非真正的设置界面，要修改
	// TODO: 此函数还没有链接到信号
	public void OnButtonPressed()
	{
		GD.Print("假装打开了设置捏");
	}
	
	
}
