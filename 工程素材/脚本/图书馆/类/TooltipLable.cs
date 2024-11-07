using Godot;

public abstract partial class TooltipLable : HoverLable
{
    public override void _Ready()
    {
        base._Ready();
        
        this.MouseEntered += ShowTooltip;
        this.MouseExited += RevertShowTooltip;
    }

    public abstract void ShowTooltip();
    public abstract void RevertShowTooltip();
}