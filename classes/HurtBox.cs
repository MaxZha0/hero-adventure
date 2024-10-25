using Godot;
using System;

public partial class HurtBox : Area2D
{
	// 声明信号（委托）
	[Signal]
	public delegate void HurtEventHandler(HitBox hitBox);
}
