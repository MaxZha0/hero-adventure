using Godot;
using System;

public partial class MainPlayer : CharacterBody2D
{
	public const float speed = 200.0f;
	public const float jumpVelocity = -300.0f;

	private AnimationPlayer animPlayer;
	private Sprite2D sprite2D;

	public override void _Ready()
	{
		base._Ready();
		animPlayer = GetNode<AnimationPlayer>("AnimPlayer");
		sprite2D = GetNode<Sprite2D>("Sprite2D");
	}


	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;
		Vector2 direction = Input.GetVector("move_left", "move_right", "ui_up", "ui_down");
		if (direction != Vector2.Zero)
		{
			velocity.X = direction.X * speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, speed);
		}

		velocity += GetGravity() * (float)delta;
		velocity.X = direction.X * speed;

		if (Input.IsActionJustPressed("jump") && IsOnFloor())
		{
			velocity.Y = jumpVelocity;
		}
		Velocity = velocity;
		MoveAndSlide();


		// 播放动画
		if (IsOnFloor())
		{
			if (direction.IsZeroApprox())
			{
				animPlayer.Play("idle");
			}
			else
			{
				animPlayer.Play("running");
			}
		}
		else
		{
			animPlayer.Play("jump");
		}

		// 动画方向
		if (!direction.IsZeroApprox())
		{
			sprite2D.FlipH = direction.X < 0;
		}
	}
}
