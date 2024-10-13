using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public partial class 上下文 : Node
{
    public Vector3 position;
    public GameMaterial GroundMaterial {get; set;}
    public GameMaterial FloorMaterial {get; set;}
    
    // FIXME: 类型要改成针对的类型, 并且所有东西应该都继承自Objects类
    public List<Item> Items {get; set;}

    public 上下文(Vector3 position, GameMaterial groundMaterial, GameMaterial floorMaterial)
    {
        this.position = position;
        this.GroundMaterial = groundMaterial;
        this.FloorMaterial = floorMaterial;
        this.Items = new List<Item>();
    }

    public void AddItem(Item NewItem)
    {
        Items.Add(NewItem);
    }
}
