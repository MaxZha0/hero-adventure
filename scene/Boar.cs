using Godot;
using System;

public partial class Boar : Enemy
{
	public enum State
	{
		IDLE,
		WALK,
		RUN,
		HURT,
		DYING,
	}
	private static readonly float KNOCKBACK_VELOCITY = 300f;
	private RayCast2D wallChecker;
	private RayCast2D floorChecker;
	private RayCast2D playerChecker;
	// 无敌后的冷静时间
	private Timer calmTimer;

	public override void _Ready()
	{
		base._Ready();
		wallChecker = GetNode<RayCast2D>("Sprite2D/WallChecker");
		floorChecker = GetNode<RayCast2D>("Sprite2D/FloorChecker");
		playerChecker = GetNode<RayCast2D>("Sprite2D/PlayerChecker");
		calmTimer = GetNode<Timer>("CalmTimer");
	}

	// 检测碰撞的是不是玩家
	private bool CanSeePlayer()
	{
		if (!playerChecker.IsColliding())
		{
			return false;
		}
		return playerChecker.GetCollider() is MainPlayer;
	}

	// 根据当前人物状态，获取可能的状态改变值
	public override int GetNextState(int stateValue)
	{
		State state = (State)stateValue;
		if (enemyStats.Health <= 0)
		{ // 血量归0就死亡,如果之前是DYING，则保持当前。
			return state == State.DYING ? StateMachine.KEEP_CURRENT : (int)State.DYING;
		}

		if (pendingDamage != null)
		{
			return (int)State.HURT;
		}
		switch (state)
		{
			case State.IDLE:
				if (CanSeePlayer()) // 外界干扰的状态改变
				{
					return (int)State.RUN;
				}
				if (stateMachine.mStateTime > 2)
				{
					return (int)State.WALK; // 站立两秒就走路
				}
				break;
			case State.WALK:
				if (CanSeePlayer()) // 外界干扰的状态改变
				{
					return (int)State.RUN;
				}
				if (wallChecker.IsColliding() || !floorChecker.IsColliding())
				{
					return (int)State.IDLE; // 碰到墙或者悬崖就停止
				}
				if (stateMachine.mStateTime > 3)
				{
					return (int)State.IDLE; // 走3秒就歇会
				}
				break;
			case State.RUN:
				if (!CanSeePlayer() && calmTimer.IsStopped())
				{
					return (int)State.WALK; // 计时器清空，丢失仇恨
				}
				break;
			case State.HURT:
				if (!animPlayer.IsPlaying())
				{
					return (int)State.RUN; // 受伤动画播放完了就继续跑
				}
				break;
			case State.DYING: // 死亡时销毁节点的逻辑在动画中
				break;
		}
		return StateMachine.KEEP_CURRENT;
		// return stateValue;
	}

	// 改变状态需要做的事情
	public override void TransitionState(int fromValue, int toValue)
	{
		State from = (State)fromValue;
		State to = (State)toValue;
		GD.Print("TransitionState " + from + " to " + to);
		switch (to)
		{
			case State.IDLE:
				animPlayer.Play("idle");
				if (wallChecker.IsColliding())
				{
					ChangeDirection();
				}
				break;
			case State.WALK:
				animPlayer.Play("walk");
				if (!floorChecker.IsColliding())
				{
					ChangeDirection();
				}
				break;
			case State.RUN:
				animPlayer.Play("run");
				break;
			case State.HURT:
				animPlayer.Play("hurt");
				enemyStats.Health -= pendingDamage.Value;
				// 受到伤害的方向（伤害来源指向自己）
				Vector2 damageDir = pendingDamage.Source.GlobalPosition.DirectionTo(GlobalPosition);
				// 横向击退速度
				SetVelocityX(damageDir.X * KNOCKBACK_VELOCITY);
				// 受到伤害时，面向伤害来源
				FaceDirection = damageDir.X > 0 ? FaceDirections.LEFT : FaceDirections.RIGHT;
				// 处理完以后删掉伤害记录
				pendingDamage = null;
				break;
			case State.DYING:
				animPlayer.Play("die");
				break;
		}

	}

	// 代替原有的_PhysicsProcess,被状态机调用
	public override void TickPhysics(int stateValue, float delta)
	{
		State state = (State)stateValue;
		switch (state)
		{
			case State.HURT: // 受伤后硬直，站立不动
			case State.DYING:
			case State.IDLE:
				Move(0f, delta);
				break;
			case State.WALK:
				Move(MAX_SPEED / 3, delta);
				break;
			case State.RUN:
				if (wallChecker.IsColliding() || !floorChecker.IsColliding())
				{
					ChangeDirection();
				}
				Move(MAX_SPEED, delta);
				if (CanSeePlayer())
				{
					calmTimer.Start();
				}
				break;
		}
	}

	// 左右方向切换
	protected override void ChangeDirection()
	{
		base.ChangeDirection();
		// Godot在物理帧开始时刷新，可能会有延迟，转身后强制更新
		floorChecker.ForceRaycastUpdate();
	}

	public void OnHurt(HitBox hitBox)
	{
		// 找到玩家状态
		PlayerStats playerStats = hitBox.GetOwner().GetNode<PlayerStats>("Stats");
		// TODO pendingDamage可以优化为数组
		pendingDamage = new Damage
		{
			Value = playerStats.IsHeavyAttack ? playerStats.HeavyAttack : playerStats.Attack,
			Source = (Entity)hitBox.GetOwner()
		};
		GD.Print("小猪被攻击！" + pendingDamage.Value);
	}

	public void OnDie()
	{
		this.QueueFree();
	}
}
