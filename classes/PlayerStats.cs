using Godot;
using System;

public partial class PlayerStats : Node
{
    // 声明信号（委托）
    [Signal] public delegate void HealthChangeEventHandler();
    [Signal] public delegate void EnergyChangeEventHandler();

    private static readonly int INIT_MAX_HEALTH = 12;
    private static readonly float INIT_MAX_ENERGY = 10;
    private static readonly float DO_DAMAGE_REGION_ENERGY = 2;
    // 最大血量
    private int mMaxHealth = INIT_MAX_HEALTH;
    // 最大能量值
    private float mMaxEnergy = INIT_MAX_ENERGY;
    // 当前血量
    private int mHealth = INIT_MAX_HEALTH;
    // 当前能量（最大的一半）
    private float mEnergy = INIT_MAX_ENERGY / 2;
    // 一次滑铲消耗能量
    private float mSlidingEnergy = 4.0f;
    // 能量恢复速度
    private float mEnergyRegion = 0.8f;
    // 攻击力
    private int mAttack = 3;

    private float mHeavyDamageMultiplier = 2f;

    private bool mIsHeavyAttack = false;

    public override void _Ready()
    {
        base._Ready();
        mHealth = mMaxHealth;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        Energy += (float)(mEnergyRegion * delta);
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

    public float Energy
    {
        get => mEnergy;
        set
        {
            // 添加数据校验
            value = Mathf.Clamp(value, 0, mMaxEnergy);
            mEnergy = value;
            // 发出信号，能量变化
            this.EmitSignal("EnergyChange");
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

    public float MaxEnergy
    {
        get => mMaxEnergy;
        set
        {
            // 添加数据校验
            value = Mathf.Max(value, 0);
            mMaxEnergy = value;
            Energy = Mathf.Min(mEnergy, mMaxEnergy);
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

    // 判断是否可以滑铲
    public bool CanSlide()
    {
        return Energy >= mSlidingEnergy;
    }

    // 执行滑铲
    public void DoSlide()
    {
        Energy -= mSlidingEnergy;
    }

    // 造成伤害
    public void DoDamage()
    {
        Energy += DO_DAMAGE_REGION_ENERGY;
    }
}
