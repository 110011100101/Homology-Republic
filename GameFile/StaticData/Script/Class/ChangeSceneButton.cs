using Godot;
using System;

public abstract partial class ChangeSceneButton : TextureButton
{
    public abstract void ChangeScene();

    public override void _Pressed()
    {
        ChangeScene();
    }
}
