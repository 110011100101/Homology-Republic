using Godot;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

public partial class TitleGroup_LaunchScreen : Node2D
{
	private bool _isLoadingFinished = false;

	public async override void _Ready()
	{
		Task.Run(() => CallDeferred("_LoadingAssets"));
		Task.Run(() => _FadeIn());
	}

	public override void _Process(double delta)
	{
	}

	private Task _FadeIn()
	{
		for (float i = 0; i < 1; i += 0.01f)
		{
			CallDeferred("_SetModulate", new Color(1, 1, 1, i));
			Thread.Sleep(10);
		}

		int _ShakeHandCounter = 0;

		while (_ShakeHandCounter < 30)
		{
			if (_isLoadingFinished)
			{
				Thread.Sleep(1000);
				CallDeferred("_ChangeScene");
				return Task.CompletedTask;
			}
			_ShakeHandCounter++;
			Thread.Sleep(1000);
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

		_isLoadingFinished = true;
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
