using Godot;
using System;

public partial class PlayerStats : Node
{
    // 最大血量
    private int mMaxHealth = 12;
    // 当前血量
    private int mHealth;
    // 攻击力
    private int mAttack = 3;

    private float mHeavyDamageMultiplier = 5f;

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
