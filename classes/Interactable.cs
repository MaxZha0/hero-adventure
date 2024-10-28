using Godot;
using System;

public partial class Interactable : Area2D
{
	// 声明信号（委托）
	[Signal] public delegate void InteractEventHandler();

	public override void _Ready()
	{
		CollisionLayer = 0; // 自身处于环境层
		CollisionMask = 0;
		SetCollisionMaskValue(2, true); // 可以检测到2（玩家层）
		this.Connect("body_entered", new Callable(this, "OnBodyEntered"));
		this.Connect("body_exited", new Callable(this, "OnBodyExited"));
	}


	public void OnInteract()
	{
		GD.Print("Interact " + this.Name);
		// 发出信号，进入交互区域
		this.EmitSignal("Interact");
	}

	// 玩家进入
	public void OnBodyEntered(MainPlayer player)
	{
		player.RegisterInteractable(this); // 标记交互对象
	}

	// 玩家退出
	public void OnBodyExited(MainPlayer player)
	{
		player.UnregisterInteractable(this);
	}
}
