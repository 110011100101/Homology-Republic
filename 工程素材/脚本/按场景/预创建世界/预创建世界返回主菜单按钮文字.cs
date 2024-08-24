using Godot;
using System;

public partial class 预创建世界返回主菜单按钮文字 : Label
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
	
	// 设定按钮点击事件
	public void OnButtonPressed()
	{
		// TODO: 这里是要用的时候才加载，但其实需要提前加载好才不会卡
		GetTree().ChangeSceneToPacked((PackedScene)ResourceLoader.Load(场景路径.主菜单));
	}
}
