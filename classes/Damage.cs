using Godot;
using System;

public partial class Damage : RefCounted
{
    private int mValue;

    private Entity mSource;

    public int Value
    {
        get => mValue;
        set
        {
            // 添加数据校验
            value = Mathf.Max(value, 0);
            mValue = value;
        }
    }

    public Entity Source
    {
        get => mSource;
        set
        {
            mSource = value;
        }
    }
}
