using Godot;
using System;
using System.Linq;

public enum State
{
	IDLE,
	RUNNING,
	JUMP,
	FALL,
	LANDING,
	WALL_SLIDING,
	WALL_JUMP,
}

public partial class MainPlayer : CharacterBody2D
{
	// 移动速度
	private static readonly float SPEED = 150.0f;

	// 起跳的反向速度
	private static readonly float JUMP_VELOCITY = -310.0f;

	private static Vector2 WALL_JUMP_VELOCITY = new(1000, -310);

	// 地板上的加速度（摩擦力）
	private static readonly float FLOOR_ACCELERATION = SPEED / 0.1f;

	// 空中的加速度（翻身跳）
	private static readonly float AIR_ACCELERATION = SPEED / 0.02f;

	// 集合，代表处于地面的状态
	private static readonly State[] GROUND_STATES = new State[]
	{
		State.IDLE,
		State.RUNNING,
		State.LANDING,
	};

	private AnimationPlayer animPlayer;
	private Sprite2D sprite2D;
	// 郊狼时间
	private Timer coyoteTimer;
	// 落地起跳缓存时间
	private Timer jumpBufferTimer;
	// 是否是改变状态后的第一帧
	private bool isFirstTick = false;
	// 爬墙脚部监测点
	private RayCast2D footCheck;
	// 爬墙手部监测点
	private RayCast2D handCheck;

	private StateMachine stateMachine;

	public override void _Ready()
	{
		base._Ready();
		animPlayer = GetNode<AnimationPlayer>("AnimPlayer");
		sprite2D = GetNode<Sprite2D>("Sprite2D");
		coyoteTimer = GetNode<Timer>("CoyoteTimer");
		jumpBufferTimer = GetNode<Timer>("JumpBufferTimer");
		footCheck = GetNode<RayCast2D>("Sprite2D/footCheck");
		handCheck = GetNode<RayCast2D>("Sprite2D/handCheck");
		stateMachine = GetNode<StateMachine>("StateMachine");
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
				SetVelocityY(JUMP_VELOCITY / 2);
			}
		}
	}

	// 改变纵向速度的方法
	public void SetVelocityY(float speed)
	{
		Vector2 velocity = Velocity;
		velocity.Y = speed;
		Velocity = velocity;
	}

	// 根据当前人物状态，获取可能的状态改变值
	public State GetNextState(State stateValue)
	{
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
				if (!IsOnFloor()) // 不在地板上转为FALL
				{
					return State.FALL;
				}
				if (!isStill) // 不静止转为RUNNING
				{
					return State.RUNNING;
				}
				break;
			case State.RUNNING:
				if (!IsOnFloor()) // 不在地板上转为FALL
				{
					return State.FALL;
				}
				if (isStill) // 静止了转为IDLE
				{
					return State.IDLE;
				}
				break;
			case State.JUMP:
				if (velocity.Y >= 0) // 纵向速度为正,转为FALL
				{
					return State.FALL;
				}
				break;
			case State.FALL:
				if (IsOnFloor())
				{
					if (isStill) // 在地板上静止转为LANDING
					{
						return State.LANDING;
					}
					else // 在地板上移动则直接转为RUNNING
					{
						return State.RUNNING;
					}
				}
				else if (IsOnWall() && handCheck.IsColliding() && footCheck.IsColliding())
				{ // 如果在墙上，同时手脚监测点都触碰，则进入爬墙态
					return State.WALL_SLIDING;
				}
				break;
			case State.LANDING:
				if (!isStill)
				{
					return State.RUNNING;// 着陆时可以直接进入RUNNING
				}
				if (!animPlayer.IsPlaying())// 着陆动画播放完则转为IDLE
				{
					return State.IDLE;
				}
				break;
			case State.WALL_SLIDING:
				if (jumpBufferTimer.TimeLeft > 0)
				{
					return State.WALL_JUMP; // 在墙上滑行时按下跳跃
				}
				if (IsOnFloor())
				{
					return State.IDLE;
				}
				if (!IsOnWall())
				{
					return State.FALL;
				}
				break;
			case State.WALL_JUMP:
				if (velocity.Y >= 0) // 纵向速度为正,转为FALL(与跳跃相同)
				{
					return State.FALL;
				}
				break;
		}

		return state;
	}

	// 改变状态需要做的事情
	public void TransitionState(State from, State to)
	{
		GD.Print("TransitionState " + from + " to " + to);
		// 移动前在天上,移动后在地上,停止郊狼计数
		if (!GROUND_STATES.Contains(from) && GROUND_STATES.Contains(to))
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
				SetVelocityY(JUMP_VELOCITY);
				// 实际起跳后停止郊狼时间计时器
				coyoteTimer.Stop();
				// 实际起跳后停止起跳缓冲计时器
				jumpBufferTimer.Stop();
				break;
			case State.FALL:
				animPlayer.Play("fall");
				if (GROUND_STATES.Contains(from)) // 如果是从地面转到FALL的话,开始郊狼时间计数
				{
					coyoteTimer.Start();
				}
				break;
			case State.LANDING:
				animPlayer.Play("landing");
				break;
			case State.WALL_SLIDING:
				SetVelocityY(0);
				animPlayer.Play("wall_sliding");
				break;
			case State.WALL_JUMP:// 类似跳跃
				animPlayer.Play("jump");
				Vector2 velocity = Velocity;
				velocity = WALL_JUMP_VELOCITY; // x和y方向都有值的向量
				velocity.X *= GetWallNormal().X; // 根据墙的方向改变跳跃方向
				Velocity = velocity;
				// 实际起跳后停止起跳缓冲计时器
				jumpBufferTimer.Stop();
				break;
		}

		if (to == State.JUMP)
		{
			// animPlayer.SpeedScale = 0.3f;
		}
		if (from == State.JUMP)
		{
			animPlayer.SpeedScale = 1.0f;
		}
		// 代表状态切换以后的第一帧，为了处理竖向加速度
		isFirstTick = true;
	}

	// 代替原有的_PhysicsProcess,被状态机调用
	public void TickPhysics(State state, float delta)
	{
		// 跳跃第一帧不考虑加速度
		Vector2 targetGravity = isFirstTick ? new Vector2(0, 0) : GetGravity();
		switch (state)
		{
			case State.IDLE:
				Move(GetGravity(), delta);
				break;
			case State.RUNNING:
				Move(GetGravity(), delta);
				break;
			case State.JUMP:
				Move(targetGravity, delta);
				break;
			case State.FALL:
				Move(GetGravity(), delta);
				break;
			case State.LANDING:
				Stand(GetGravity(), delta);
				break;
			case State.WALL_SLIDING:
				Move(GetGravity() / 6, delta);
				break;
			case State.WALL_JUMP:
				if (stateMachine.mStateTime < 0.1)
				{
					Stand(targetGravity, delta);
				}
				else
				{ // 肯定不是第一帧，跳跃正常重力即可
					Move(GetGravity(), delta);
				}

				break;
		}
		isFirstTick = false;
	}

	// 移动方法
	public void Move(Vector2 gravity, float delta)
	{
		Vector2 velocity = Velocity;
		Vector2 direction = Input.GetVector("move_left", "move_right", "ui_up", "ui_down");

		// X分量目标速度
		float targetVelocity = direction.X * SPEED;
		// 地上添加摩擦力（起跑和减速），空中不添加（加速度很大），空中转身迅速
		float acceleration = IsOnFloor() ? FLOOR_ACCELERATION : AIR_ACCELERATION;
		// X分量实际速度
		velocity.X = Mathf.MoveToward(velocity.X, targetVelocity, acceleration * (float)delta);
		// Y分量持续添加重力加速度
		velocity.Y += gravity.Y * (float)delta;
		// 镜像切换动画方向
		if (!direction.IsZeroApprox())
		{
			Vector2 scale = sprite2D.Scale;
			scale.X = direction.X;
			sprite2D.Scale = scale;
		}
		Velocity = velocity;
		// 执行移动
		MoveAndSlide();
	}

	// 移动方法
	public void Sliding(Vector2 gravity, float delta)
	{
		Vector2 velocity = Velocity;
		Vector2 direction = Input.GetVector("move_left", "move_right", "ui_up", "ui_down");

		// X分量目标速度
		float targetVelocity = direction.X * SPEED;
		// 地上添加摩擦力（起跑和减速），空中不添加（加速度很大），空中转身迅速
		float acceleration = IsOnFloor() ? FLOOR_ACCELERATION : AIR_ACCELERATION;
		// X分量实际速度
		velocity.X = Mathf.MoveToward(velocity.X, targetVelocity, acceleration * (float)delta);
		// Y分量持续添加重力加速度
		velocity.Y += gravity.Y * (float)delta;
		// 镜像切换动画方向
		if (!direction.IsZeroApprox())
		{
			Vector2 scale = sprite2D.Scale;
			scale.X = direction.X;
			sprite2D.Scale = scale;
		}
		Velocity = velocity;
		// 执行移动
		MoveAndSlide();
	}

	// 站立方法
	public void Stand(Vector2 gravity, float delta)
	{
		Vector2 velocity = Velocity;
		// 地上添加摩擦力（起跑和减速），空中不添加（加速度很大），空中转身迅速
		float acceleration = IsOnFloor() ? FLOOR_ACCELERATION : AIR_ACCELERATION;
		// X分量 速度降到0
		velocity.X = Mathf.MoveToward(velocity.X, 0, acceleration * (float)delta);
		// Y分量持续添加重力加速度
		velocity.Y += gravity.Y * (float)delta;
		Velocity = velocity;
		// 执行移动
		MoveAndSlide();
	}
}
