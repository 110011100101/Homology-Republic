using Godot;

public abstract partial class Item : Sprite2D
{
    public abstract GameMaterial ItemMaterial {get; set;}

    public abstract void ChangeItemMaterial(GameMaterial targetMaterial);
}