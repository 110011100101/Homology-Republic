using Godot;
using System;

public partial class 继续游戏按钮文字 : Label
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
		// 设定不同情况下Lable的坐标(我把他们攒在一起是因为没什么好读的)
		if (ExitButton.ButtonPressed) Position = PredssedPosition;
		else if (ExitButton.IsHovered()) Position = HoverPosition;
		else Position = NormalPosition;
	}

	// 这里定义继续游戏按钮的点击事件
	// TODO: 还没做存档系统
	// 预期效果是，在Logo界面提前加载游戏，然后主界面的背景是真实的游戏画面，点击继续游戏直接就进入游戏
	// 这里先用一个空的函数代替
	// 注意，这个函数是TextureButton的，不是Label的
	public void OnButtonPressed()
	{
	}
}
