using Godot;
using System;

public partial class Notice : RichTextLabel
{
	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
	}

	public void OutputNotice(string notice)
	{
		this.AddText("\n" + notice);
	}
}
