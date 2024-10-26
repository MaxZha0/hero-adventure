using Godot;
using System;

public enum FaceDirections
{
    RIGHT = 1,
    LEFT = -1,
}

public partial class Enemy : Entity
{
    // 面朝方向
    [Export] protected FaceDirections direction = FaceDirections.LEFT;
    // 最大速度(比玩家快一点)
    protected static readonly float MAX_SPEED = 100f;

    // 地板上的加速度（摩擦力）
    protected static readonly float ACCELERATION = MAX_SPEED / 0.1f;

    protected AnimationPlayer animPlayer;
    protected Sprite2D sprite2D;
    protected StateMachine stateMachine;
    // 当前敌人的状态
    public EnemyStats enemyStats;
    // 受到的伤害
    protected Damage pendingDamage;


    // 本质上是个getter和setter的集合体
    protected FaceDirections FaceDirection
    {
        get => direction;
        set
        {
            direction = value;
            // 根据运动方向改变图片朝向（受素材限制）
            Vector2 scale = sprite2D.Scale;
            scale.X = -(int)direction;
            sprite2D.Scale = scale;
        }
    }

    public override void _Ready()
    {
        base._Ready();
        animPlayer = GetNode<AnimationPlayer>("AnimPlayer");
        sprite2D = GetNode<Sprite2D>("Sprite2D");
        stateMachine = GetNode<StateMachine>("StateMachine");
        enemyStats = GetNode<EnemyStats>("Stats");
    }

    // 左右方向切换
    protected virtual void ChangeDirection()
    {
        FaceDirection = (FaceDirections)(-(int)FaceDirection);
    }

    // 移动方法
    protected void Move(float speed, float delta)
    {
        Vector2 velocity = Velocity;
        // X分量实际速度
        velocity.X = Mathf.MoveToward(velocity.X, speed * (int)direction, ACCELERATION * (float)delta);
        // Y分量持续添加重力加速度
        velocity.Y += GetGravity().Y * (float)delta;
        Velocity = velocity;
        // 执行移动
        MoveAndSlide();
    }
}
