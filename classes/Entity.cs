using Godot;
using System;

public partial class Entity : CharacterBody2D
{
    // 根据当前人物状态，获取可能的状态改变值
    public virtual int GetNextState(int stateValue)
    {
        return stateValue;
    }

    // 改变状态需要做的事情
    public virtual void TransitionState(int fromValue, int toValue)
    {
    }

    // 代替原有的_PhysicsProcess,被状态机调用
    public virtual void TickPhysics(int stateValue, float delta)
    {
    }

    // 改变纵向速度的方法
    public void SetVelocityY(float speed)
    {
        Vector2 velocity = Velocity;
        velocity.Y = speed;
        Velocity = velocity;
    }

    // 改变横向向速度的方法
    public void SetVelocityX(float speed)
    {
        Vector2 velocity = Velocity;
        velocity.X = speed;
        Velocity = velocity;
    }

}
