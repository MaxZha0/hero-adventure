using Godot;
using System;
using System.Linq;

public partial class MainPlayer : Entity
{
	public enum State
	{
		IDLE,
		RUNNING,
		JUMP,
		FALL,
		LANDING,
		WALL_SLIDING,
		WALL_JUMP,
		ATTACK_1,
		ATTACK_2,
		HEAVY_ATTACK,
		HURT,
		DYING,
	}
	// 移动速度
	private static readonly float SPEED = 150.0f;

	// 起跳的反向速度
	private static readonly float JUMP_VELOCITY = -310.0f;

	private static Vector2 WALL_JUMP_VELOCITY = new(380, -280);

	// 地板上的加速度（摩擦力）
	private static readonly float FLOOR_ACCELERATION = SPEED / 0.1f;

	// 空中的加速度（翻身跳）
	private static readonly float AIR_ACCELERATION = SPEED / 0.1f;

	// 集合，代表处于地面的状态
	private static readonly State[] GROUND_STATES = new State[]
	{
		State.IDLE,
		State.RUNNING,
		State.LANDING,
		State.ATTACK_1,
		State.ATTACK_2,
		State.HEAVY_ATTACK,
	};
	// 攻击是否能链接
	[Export] private bool CanCombo = false;
	// 是否触发连击
	private bool isComboRequest = false;
	private AnimationPlayer animPlayer;
	private Sprite2D sprite2D;
	// 郊狼时间
	private Timer coyoteTimer;
	// 落地起跳缓存时间
	private Timer jumpBufferTimer;
	// 受伤后无敌时间
	private Timer invincibleTimer;
	// 是否是改变状态后的第一帧
	private bool isFirstTick = false;
	// 爬墙脚部监测点
	private RayCast2D footCheck;
	// 爬墙手部监测点
	private RayCast2D handCheck;

	private StateMachine stateMachine;

	private PlayerStats playerStats;

	// 受到的伤害
	private Damage pendingDamage;

	public override void _Ready()
	{
		base._Ready();
		animPlayer = GetNode<AnimationPlayer>("AnimPlayer");
		sprite2D = GetNode<Sprite2D>("Sprite2D");
		coyoteTimer = GetNode<Timer>("CoyoteTimer");
		jumpBufferTimer = GetNode<Timer>("JumpBufferTimer");
		invincibleTimer = GetNode<Timer>("InvincibleTimer");
		footCheck = GetNode<RayCast2D>("Sprite2D/footCheck");
		handCheck = GetNode<RayCast2D>("Sprite2D/handCheck");
		stateMachine = GetNode<StateMachine>("StateMachine");
		playerStats = GetNode<PlayerStats>("Stats");
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
		if (Input.IsActionJustReleased("attack") && CanCombo)
		{
			isComboRequest = true;
		}

	}

	// 根据当前人物状态，获取可能的状态改变值
	public override int GetNextState(int stateValue)
	{
		State state = (State)stateValue;
		if (playerStats.Health <= 0)
		{ // 血量归0就死亡,如果之前是DYING，则保持当前。
			return state == State.DYING ? StateMachine.KEEP_CURRENT : (int)State.DYING;
		}
		if (pendingDamage != null)
		{
			return (int)State.HURT;
		}

		// 起跳条件：在地板上 或者 处于郊狼时间
		bool canJump = IsOnFloor() || coyoteTimer.TimeLeft > 0;
		// 如果能起跳，并且在起跳缓冲内，则允许起跳
		bool shouldJump = canJump && jumpBufferTimer.TimeLeft > 0;
		if (shouldJump)
		{
			return (int)State.JUMP;
		}
		if (GROUND_STATES.Contains(state) && !IsOnFloor())// 在地上且地下没地板时下落
		{
			return (int)State.FALL;
		}
		Vector2 velocity = Velocity;
		Vector2 direction = Input.GetVector("move_left", "move_right", "ui_up", "ui_down");
		// IDLE状态：无按键并且X速度为0
		bool isStill = direction.IsZeroApprox() && velocity.X.Equals(0);
		switch (state)
		{
			case State.IDLE:
				if (Input.IsActionJustReleased("attack"))
				{
					return (int)State.ATTACK_1; // 按J攻击
				}
				if (Input.IsActionJustReleased("heavy_attack"))
				{
					return (int)State.HEAVY_ATTACK; // 按k重击
				}
				if (!isStill) // 不静止转为RUNNING
				{
					return (int)State.RUNNING;
				}
				break;
			case State.RUNNING:
				if (Input.IsActionJustReleased("attack"))
				{
					return (int)State.ATTACK_1; // 按J攻击
				}
				if (Input.IsActionJustReleased("heavy_attack"))
				{
					return (int)State.HEAVY_ATTACK; // 按k重击
				}
				if (isStill) // 静止了转为IDLE
				{
					return (int)State.IDLE;
				}
				break;
			case State.JUMP:
				if (velocity.Y >= 0) // 纵向速度为正,转为FALL
				{
					return (int)State.FALL;
				}
				break;
			case State.FALL:
				if (IsOnFloor())
				{
					if (isStill) // 在地板上静止转为LANDING
					{
						return (int)State.LANDING;
					}
					else // 在地板上移动则直接转为RUNNING
					{
						return (int)State.RUNNING;
					}
				}
				else if (IsOnWall() && handCheck.IsColliding() && footCheck.IsColliding())
				{ // 如果在墙上，同时手脚监测点都触碰，则进入爬墙态
					return (int)State.WALL_SLIDING;
				}
				break;
			case State.LANDING:
				if (!isStill)
				{
					return (int)State.RUNNING;// 着陆时可以直接进入RUNNING
				}
				if (!animPlayer.IsPlaying())// 着陆动画播放完则转为IDLE
				{
					return (int)State.IDLE;
				}
				break;
			case State.WALL_SLIDING:
				if (jumpBufferTimer.TimeLeft > 0)
				{
					return (int)State.WALL_JUMP; // 在墙上滑行时按下跳跃
				}
				if (IsOnFloor())
				{
					return (int)State.IDLE;
				}
				if (!footCheck.IsColliding())
				{
					return (int)State.FALL; // 脚没踩住墙，从墙上掉落时，恢复FALL
				}
				// if (!IsOnWall())
				// {
				// 当前版本Godot有Bug，在右边墙上的时候，不能保持IsOnWall状态
				// return State.FALL;
				// }
				break;
			case State.WALL_JUMP:
				if (velocity.Y >= 0) // 纵向速度为正,转为FALL(与跳跃相同)
				{
					return (int)State.FALL;
				}
				break;

			case State.ATTACK_1:
				if (!animPlayer.IsPlaying())
				{ // 动画播放完 链接2
					return isComboRequest ? (int)State.ATTACK_2 : (int)State.IDLE;
				}
				break;
			case State.ATTACK_2:
				if (!animPlayer.IsPlaying())
				{ // 动画播放完 链接3
					return (int)State.IDLE;
				}
				break;
			case State.HEAVY_ATTACK:
				if (!animPlayer.IsPlaying())
				{ // 动画播放完 结束
					playerStats.IsHeavyAttack = false; // 标记重击结束
					return (int)State.IDLE;
				}
				break;
			case State.HURT:
				if (!animPlayer.IsPlaying())
				{
					return (int)State.IDLE; // 受伤动画播放完了就IDLE
				}
				break;
			case State.DYING: // 死亡时销毁节点的逻辑在动画中
				break;
		}

		return StateMachine.KEEP_CURRENT;
	}

	// 改变状态需要做的事情
	public override void TransitionState(int fromValue, int toValue)
	{
		State from = (State)fromValue;
		State to = (State)toValue;
		// GD.Print("TransitionState " + from + " to " + to);
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
				Vector2 velocity = WALL_JUMP_VELOCITY; // x和y方向都有值的向量
				velocity.X *= GetWallNormal().X; // 根据墙的方向改变跳跃方向
				Velocity = velocity;
				// 实际起跳后停止起跳缓冲计时器
				jumpBufferTimer.Stop();
				break;
			case State.ATTACK_1:
				animPlayer.Play("attack_1");
				isComboRequest = false;
				break;
			case State.ATTACK_2:
				animPlayer.Play("attack_2");
				isComboRequest = false;
				break;
			case State.HEAVY_ATTACK:
				playerStats.IsHeavyAttack = true; // 标记重击
				animPlayer.Play("attack_3");
				isComboRequest = false;
				break;
			case State.HURT:
				animPlayer.Play("hurt");
				playerStats.Health -= pendingDamage.Value;
				// 受到伤害的方向（伤害来源指向自己）
				Vector2 damageDir = pendingDamage.Source.GlobalPosition.DirectionTo(GlobalPosition);
				// 横向击退速度
				SetVelocityX(damageDir.X * 300);
				// 处理完以后删掉伤害记录
				pendingDamage = null;
				// 受伤后无敌
				invincibleTimer.Start();
				break;
			case State.DYING:
				animPlayer.Play("die");
				// 死亡后停止计时
				invincibleTimer.Stop();
				break;
		}
		// 代表状态切换以后的第一帧，为了处理竖向加速度
		isFirstTick = true;
	}

	// 代替原有的_PhysicsProcess,被状态机调用
	public override void TickPhysics(int stateValue, float delta)
	{
		State state = (State)stateValue;
		// 如果受伤，则闪烁
		SetPlayerHurtWarning();
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
				// 滑墙状态不接受左右方向干扰
				Stand(GetGravity() / 10, delta);
				break;
			case State.WALL_JUMP:
				// 肯定不是第一帧，跳跃正常重力即可
				Move(GetGravity(), delta);
				break;
			case State.ATTACK_1:
			case State.ATTACK_2:
			case State.HEAVY_ATTACK:
				Stand(GetGravity(), delta);
				break;
			case State.HURT: // 受伤后硬直，站立不动
			case State.DYING:
				Stand(GetGravity(), delta);
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

	// 站立方法 在墙上滑行时靠这个方法屏蔽按键（同时控制朝向）
	public void Stand(Vector2 gravity, float delta)
	{
		Vector2 velocity = Velocity;
		// 地上添加摩擦力（起跑和减速），空中不添加（加速度很大），空中转身迅速
		float acceleration = IsOnFloor() ? FLOOR_ACCELERATION : AIR_ACCELERATION;
		// X分量 速度降到0
		velocity.X = Mathf.MoveToward(velocity.X, 0, acceleration * (float)delta);
		// Y分量持续添加重力加速度
		velocity.Y += gravity.Y * (float)delta;

		// 镜像切换动画方向
		if (velocity.X != 0)
		{
			Vector2 scale = sprite2D.Scale;
			scale.X = velocity.X > 0 ? 1 : -1;
			sprite2D.Scale = scale;
		}
		Velocity = velocity;
		// 执行移动
		MoveAndSlide();
	}

	public void OnHurt(HitBox hitBox)
	{
		if (invincibleTimer.TimeLeft > 0)
		{
			return; // 无敌期间不处理受伤状态
		}
		// 找到敌人状态
		EnemyStats enemyStats = hitBox.GetOwner().GetNode<EnemyStats>("Stats");
		GD.Print("OnHurt " + hitBox.GetOwner().Name);

		// TODO pendingDamage可以优化为数组
		pendingDamage = new Damage
		{
			Value = enemyStats.Attack,
			Source = (Entity)hitBox.GetOwner()
		};
		GD.Print("受伤！" + pendingDamage.Value);
	}

	public void OnPlayerDie()
	{
		// 重新加载当前场景
		GetTree().ReloadCurrentScene();
	}

	// 受伤后添加闪烁效果，原理为给画面叠加闪烁的图层
	public void SetPlayerHurtWarning()
	{
		Color color = sprite2D.Modulate;
		if (invincibleTimer.TimeLeft > 0)
		{
			color.A = (float)(Mathf.Sin(Time.GetTicksMsec() / 20) * 0.5 + 0.5);
		}
		else
		{
			color.A = 1f;
		}
		sprite2D.Modulate = color;
	}
}
