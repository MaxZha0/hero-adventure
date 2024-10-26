using Godot;
using System;

public partial class HitBox : Area2D
{
	// 声明信号（委托）
	[Signal] public delegate void HitEventHandler(HurtBox hurtBox);

	private CollisionObject2D heavyBox;

	public override void _Ready()
	{
		// 动态链接信号（把节点的area_entered信号连接到当前节点的OnAreaEntered方法）
		this.Connect("area_entered", new Callable(this, "OnAreaEntered"));
	}

	public void OnAreaEntered(HurtBox hurtBox)
	{
		GD.Print("发生攻击：" + this.GetOwner().Name + " hit " + hurtBox.GetOwner().Name);
		// 发射信号(我打到了hurtBox)
		this.EmitSignal("Hit", hurtBox);
		// 受击方发送信号（我被打到了）
		hurtBox.EmitSignal("Hurt", this);
	}
}
