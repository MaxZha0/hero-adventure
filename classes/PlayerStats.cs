using Godot;
using Godot.Collections;
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

    public float HeavyDamageMultiplier
    {
        get => mHeavyDamageMultiplier;
        set
        {
            // 添加数据校验
            value = Mathf.Max(value, 0);
            mHeavyDamageMultiplier = value;
        }
    }

    public float SlidingEnergy
    {
        get => mSlidingEnergy;
        set
        {
            // 添加数据校验
            value = Mathf.Max(value, 0);
            mSlidingEnergy = value;
        }
    }

    public float EnergyRegion
    {
        get => mEnergyRegion;
        set
        {
            // 添加数据校验
            value = Mathf.Max(value, 0);
            mEnergyRegion = value;
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

    // 临时把场景中所有的数据都写入字典
    public Dictionary ToDict()
    {
        return new Dictionary()
        {
            {MaxEnergy,MaxEnergy},
            {"Energy",Energy},
            {"MaxHealth",MaxHealth},
            {"Health",Health},
            {"Attack",Attack},
            {"HeavyDamageMultiplier",HeavyDamageMultiplier},
            {"SlidingEnergy",SlidingEnergy},
            {"EnergyRegion",EnergyRegion},
        };
    }

    // 从字典中读取保存的数据
    public void FromDict(Dictionary dict)
    {

        if (dict.ContainsKey(MaxEnergy))
            MaxEnergy = (float)dict["MaxEnergy"];

        if (dict.ContainsKey("Energy"))
            Energy = (float)dict["Energy"];

        if (dict.ContainsKey("MaxHealth"))
            MaxHealth = (int)dict["MaxHealth"];

        if (dict.ContainsKey("Health"))
            Health = (int)dict["Health"];

        if (dict.ContainsKey("Attack"))
            Attack = (int)dict["Attack"];

        if (dict.ContainsKey("HeavyDamageMultiplier"))
            HeavyDamageMultiplier = (float)dict["HeavyDamageMultiplier"];

        if (dict.ContainsKey("SlidingEnergy"))
            SlidingEnergy = (float)dict["SlidingEnergy"];

        if (dict.ContainsKey("EnergyRegion"))
            EnergyRegion = (float)dict["EnergyRegion"];
    }


    // // 临时把场景中所有的数据都写入字典
    // public Dictionary<string, List<string>> ToDict()
    // {
    //     List<string> enemiesAlive = new();
    //     // 根据敌人分组，便利所有状态，保存
    //     foreach (Enemy enemy in GetTree().GetNodesInGroup("enermy"))
    //     {
    //         string path = GetPathTo(enemy);
    //         enemiesAlive.Add(path);
    //     }
    //     return new Dictionary<string, List<string>>()
    //     {
    //         {"enemiesAlive",enemiesAlive}
    //     };

    // }

    // // 从字典中读取保存的数据
    // public void FromDict(Dictionary<string, List<string>> dic)
    // {
    //     foreach (Enemy enemy in GetTree().GetNodesInGroup("enermy"))
    //     {
    //         var path = GetPathTo(enemy);
    //         List<string> enemiesAlive = new();
    //         if (!dic.TryGetValue("enemiesAlive", out enemiesAlive))
    //         {
    //             return;
    //         }
    //         if (!enemiesAlive.Contains(path))
    //         {
    //             enemy.QueueFree();
    //         }
    //     }
    // }
}
