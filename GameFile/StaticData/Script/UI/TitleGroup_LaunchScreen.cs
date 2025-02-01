using Godot;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

public partial class TitleGroup_LaunchScreen : Node2D
{
	private bool isLoadingFinished = false;

	public override void _Ready()
	{
		_ = Task.Run(() => CallDeferred("_LoadingAssets"));
		_ = Task.Run(() => _FadeIn());
	}

	public override void _Process(double delta)
	{
	}

	private Task _FadeIn()
	{
		for (float i = 0; i < 1; i += 0.01f)
		{
			CallDeferred("_SetModulate", new Color(1, 1, 1, i));
			Thread.Sleep(30);
		}

		int _ShakeHandCounter = 0;

		while (_ShakeHandCounter < 30)
		{
			if (isLoadingFinished)
			{
				Thread.Sleep(500);
				CallDeferred("_ChangeScene");
				return Task.CompletedTask;
			}
			_ShakeHandCounter++;
			Thread.Sleep(500);
		}
		CallDeferred("_DefferredTimeOutError");
		return Task.CompletedTask;

	}

	private void _LoadingAssets()
	{
		// TODO: 在这加载所有资源
		GetNode<Data>("/root/Data").MainMenu = GD.Load<PackedScene>(ScenePath.MainMenu);
		GetNode<Data>("/root/Data").CreatePlanetMenu = GD.Load<PackedScene>(ScenePath.CreatePlanetMenu);
		GetNode<Data>("/root/Data").BasePlanetCreatingMenu = GD.Load<PackedScene>(ScenePath.BasePlanetCreatingMenu);
		GetNode<Data>("/root/Data").SettingMenu = GD.Load<PackedScene>(ScenePath.SettingMenu);
		GetNode<Data>("/root/Data").PlayGround = GD.Load<PackedScene>(ScenePath.PlayGround);
		GetNode<Data>("/root/Data").Tiles = ResourceLoader.Load<TileSet>("res://GameFile/StaticData/GameAssets/Texture/GridConceptPack/tiles.tres"); // FIXME: 这里没有实现更改材质包的功能
		GetNode<Data>("/root/Data").Topography = ResourceLoader.Load<TileSet>("res://GameFile/StaticData/GameAssets/Texture/GridConceptPack/topography.tres"); // FIXME: 这里没有实现更改材质包的功能

		isLoadingFinished = true;
	}

	private void _ChangeScene()
	{
		GetTree().ChangeSceneToPacked(GetNode<Data>("/root/Data").MainMenu);
	}

	private void _SetModulate(Color color)
	{
		this.Modulate = color;
	}

	private void _DefferredTimeOutError()
	{
		KTError.PrintError("KT0001", this, ((Script)GetScript()).ResourcePath);
	}
}
