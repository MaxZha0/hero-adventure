using Godot;
using System;

public partial class EnemyStats : Node
{
    // 最大血量
    private int mMaxHealth = 70;
    // 当前血量
    private int mHealth;
    // 攻击力
    private int mAttack = 3;

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
            // 添加数据校验 0到MAX之间
            value = Mathf.Clamp(value, 0, mMaxHealth);
            mHealth = value;
        }
    }

    public int Attack
    {
        get => mAttack;
        set
        {
            // 添加数据校验 大于等于0
            value = Mathf.Max(value, 0);
            mAttack = value;
        }
    }
}
