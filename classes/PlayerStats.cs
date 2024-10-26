using Godot;
using System;

public partial class PlayerStats : Node
{
    // 声明信号（委托）
    [Signal] public delegate void HealthChangeEventHandler();

    private static readonly int INIT_MAX_HEALTH = 12;
    // 最大血量
    private int mMaxHealth = INIT_MAX_HEALTH;
    // 当前血量
    private int mHealth = INIT_MAX_HEALTH;
    // 攻击力
    private int mAttack = 3;

    private float mHeavyDamageMultiplier = 2f;

    private bool mIsHeavyAttack = false;

    public override void _Ready()
    {
        base._Ready();
        mHealth = mMaxHealth;
    }

    public int Health
    {
        get => mHealth;
        set
        {
            // 添加数据校验
            value = Mathf.Clamp(value, 0, mMaxHealth);
            mHealth = value;
            // 发出信号，生命变化
            this.EmitSignal("HealthChange");
        }
    }
    public int MaxHealth
    {
        get => mMaxHealth;
        set
        {
            // 添加数据校验
            value = Mathf.Max(value, 0);
            mMaxHealth = value;
            // 如果当前血量大于最大血量，则降到一样
            Health = Mathf.Min(mHealth, mMaxHealth);
        }
    }

    public int Attack
    {
        get => mAttack;
        set
        {
            // 添加数据校验
            value = Mathf.Max(value, 0);
            mAttack = value;
        }
    }

    public int HeavyAttack
    {
        get => (int)(mAttack * mHeavyDamageMultiplier + 0.5);
    }

    public bool IsHeavyAttack
    {
        get => mIsHeavyAttack;
        set
        {
            mIsHeavyAttack = value;
        }
    }
}
