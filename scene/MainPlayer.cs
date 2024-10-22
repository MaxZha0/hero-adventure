using Godot;
using System;

public enum State
{
	IDLE = 0,
	RUNNING = 1,
	JUMP = 2,
	FALL = 3,
}

public partial class MainPlayer : CharacterBody2D
{
	private static readonly float SPEED = 150.0f;

	private static readonly float JUMP_VELOCITY = -310.0f;

	// 地板上的加速度（摩擦力）
	private static readonly float FLOOR_ACCELERATION = SPEED / 0.1f;

	// 空中的加速度（翻身跳）
	private static readonly float AIR_ACCELERATION = SPEED / 0.02f;

	private static readonly State[] GROUND_STATES = new State[]
	{
		State.IDLE,
		State.RUNNING,
	};

	private AnimationPlayer animPlayer;
	private Sprite2D sprite2D;
	// 郊狼时间
	private Timer coyoteTimer;
	// 落地起跳缓存时间
	private Timer jumpBufferTimer;

	public override void _Ready()
	{
		base._Ready();
		animPlayer = GetNode<AnimationPlayer>("AnimPlayer");
		sprite2D = GetNode<Sprite2D>("Sprite2D");
		coyoteTimer = GetNode<Timer>("CoyoteTimer");
		jumpBufferTimer = GetNode<Timer>("JumpBufferTimer");
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		base._UnhandledInput(@event);
		// 每一次没有处理的起跳动作，都会开启单独计时器
		// 此操作大大提升连跳手感
		if (Input.IsActionJustPressed("jump"))
		{
			jumpBufferTimer.Start();
		}

		if (Input.IsActionJustReleased("jump"))
		{
			jumpBufferTimer.Stop();
			// 如果松开起跳键时速度还大于起跳速度的一半，则直接降低速度，起到大小跳的区别
			if (Velocity.Y < JUMP_VELOCITY / 2)
			{
				Vector2 velocity = Velocity;
				velocity.Y = JUMP_VELOCITY / 2;
				Velocity = velocity;
			}
		}
	}


	// public override void _PhysicsProcess(double delta)
	// {
	// 	Vector2 velocity = Velocity;
	// 	Vector2 direction = Input.GetVector("move_left", "move_right", "ui_up", "ui_down");

	// 	// X分量目标速度
	// 	float targetVelocity = direction.X * SPEED;
	// 	// 地上添加摩擦力（起跑和减速），空中不添加（加速度很大），空中转身迅速
	// 	float acceleration = IsOnFloor() ? FLOOR_ACCELERATION : AIR_ACCELERATION;
	// 	// X分量实际速度
	// 	velocity.X = Mathf.MoveToward(velocity.X, targetVelocity, FLOOR_ACCELERATION * (float)delta);

	// 	// Y分量持续添加重力加速度
	// 	velocity.Y += GetGravity().Y * (float)delta;
	// 	// 起跳条件：在地板上 或者 处于郊狼时间
	// 	bool canJump = IsOnFloor() || coyoteTimer.TimeLeft > 0;
	// 	// 如果能起跳，并且在起跳缓冲内，则允许起跳
	// 	bool shouldJump = canJump && jumpBufferTimer.TimeLeft > 0;
	// 	// 如果起跳成功，Y分量添加反方向速度
	// 	if (shouldJump)
	// 	{
	// 		velocity.Y = JUMP_VELOCITY;
	// 		// 实际起跳后停止郊狼时间计时器
	// 		coyoteTimer.Stop();
	// 		// 实际起跳后停止起跳缓冲计时器
	// 		jumpBufferTimer.Stop();
	// 	}
	// 	Velocity = velocity;

	// 	// 移动前是否在地板上
	// 	bool wasOnFloor = IsOnFloor();
	// 	// 执行移动
	// 	MoveAndSlide();
	// 	// 移动后如果地板状态改变，说明发生了跳跃或者坠落
	// 	if (IsOnFloor() != wasOnFloor)
	// 	{
	// 		// 如果不在地板同时不是主动起跳，则开始计时
	// 		if (!IsOnFloor() && !shouldJump)
	// 		{
	// 			coyoteTimer.Start();
	// 		}
	// 		else
	// 		{
	// 			coyoteTimer.Stop();
	// 		}
	// 	}


	// 	// 播放动画
	// 	if (IsOnFloor())
	// 	{
	// 		// 播放IDLE动画：无按键并且X速度为0
	// 		if (direction.IsZeroApprox() && velocity.X.Equals(0))
	// 		{
	// 			animPlayer.Play("idle");
	// 		}
	// 		else
	// 		{
	// 			animPlayer.Play("running");
	// 		}
	// 	}
	// 	else if (velocity.Y < 0)
	// 	{
	// 		animPlayer.Play("jump");
	// 	}
	// 	else
	// 	{
	// 		animPlayer.Play("fall");
	// 	}

	// 	// 镜像切换动画方向
	// 	if (!direction.IsZeroApprox())
	// 	{
	// 		sprite2D.FlipH = direction.X < 0;
	// 	}
	// }


	public State GetNextState(State stateValue)
	{
		// GD.Print("GetNextState");
		// 起跳条件：在地板上 或者 处于郊狼时间
		bool canJump = IsOnFloor() || coyoteTimer.TimeLeft > 0;
		// 如果能起跳，并且在起跳缓冲内，则允许起跳
		bool shouldJump = canJump && jumpBufferTimer.TimeLeft > 0;
		if (shouldJump)
		{
			return State.JUMP;
		}

		Vector2 velocity = Velocity;
		Vector2 direction = Input.GetVector("move_left", "move_right", "ui_up", "ui_down");
		// IDLE状态：无按键并且X速度为0
		bool isStill = direction.IsZeroApprox() && velocity.X.Equals(0);
		State state = stateValue;
		switch (state)
		{
			case State.IDLE:
				if (!IsOnFloor())
				{
					return State.FALL;
				}
				if (!isStill)
				{
					return State.RUNNING;
				}
				break;
			case State.RUNNING:
				if (!IsOnFloor())
				{
					return State.FALL;
				}
				if (isStill)
				{
					return State.IDLE;
				}
				break;
			case State.JUMP:
				if (velocity.Y >= 0)
				{
					return State.FALL;
				}
				break;
			case State.FALL:
				if (IsOnFloor())
				{
					if (isStill)
					{
						return State.IDLE;
					}
					else
					{
						return State.RUNNING;
					}
				}
				break;
		}

		return state;
	}

	public void TransitionState(State from, State to)
	{
		GD.Print("TransitionState");
		Vector2 velocity = Velocity;
		// from不在地上但是to在
		if (!Enum.IsDefined(typeof(State), from) && Enum.IsDefined(typeof(State), to))
		{
			coyoteTimer.Stop();
		}

		switch (to)
		{
			case State.IDLE:
				animPlayer.Play("idle");
				break;
			case State.RUNNING:
				animPlayer.Play("running");
				break;
			case State.JUMP:
				animPlayer.Play("jump");
				velocity.Y = JUMP_VELOCITY;
				// 实际起跳后停止郊狼时间计时器
				coyoteTimer.Stop();
				// 实际起跳后停止起跳缓冲计时器
				jumpBufferTimer.Stop();
				break;
			case State.FALL:
				animPlayer.Play("fall");
				if (Enum.IsDefined(typeof(State), from))
				{
					coyoteTimer.Start();
				}
				break;
		}
	}

	public void TickPhysics(State state, float delta)
	{
		GD.Print("TickPhysics " + state);
		switch (state)
		{
			case State.IDLE:
				Move(delta);
				break;
			case State.RUNNING:
				Move(delta);
				break;
			case State.JUMP:
				Move(delta);
				break;
			case State.FALL:
				Move(delta);
				break;
		}
	}

	public void Move(float delta)
	{
		Vector2 velocity = Velocity;
		Vector2 direction = Input.GetVector("move_left", "move_right", "ui_up", "ui_down");

		// X分量目标速度
		float targetVelocity = direction.X * SPEED;
		// 地上添加摩擦力（起跑和减速），空中不添加（加速度很大），空中转身迅速
		float acceleration = IsOnFloor() ? FLOOR_ACCELERATION : AIR_ACCELERATION;
		// X分量实际速度
		velocity.X = Mathf.MoveToward(velocity.X, targetVelocity, FLOOR_ACCELERATION * (float)delta);

		// Y分量持续添加重力加速度
		velocity.Y += GetGravity().Y * (float)delta;

		// 镜像切换动画方向
		if (!direction.IsZeroApprox())
		{
			sprite2D.FlipH = direction.X < 0;
		}
		Velocity = velocity;
		// 执行移动
		MoveAndSlide();
	}
}


// switch (to)
// {
// 	case State.IDLE:

// 		break;
// 	case State.RUNNING:

// 		break;
// 	case State.JUMP:

// 		break;
// 	case State.FALL:

// 		break;
// }
