using Godot;
using System;

public partial class HoverLable : RichTextLabel
{
    public override void _Ready()
    {
        this.MouseEntered += Hover;
        this.MouseExited += RevertHover;
    }

    public void Hover()
    {
        AddThemeColorOverride("default_color", new Color(0, 0.569f, 1));
    }

    public void RevertHover()
    {
        RemoveThemeColorOverride("default_color");
    }
}
