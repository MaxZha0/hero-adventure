using Godot;
using System;

public partial class Boar : Enemy
{
    public enum State
    {
        IDLE,
        WALK,
        RUN,
        HITTED,
    }
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

    // 根据当前人物状态，获取可能的状态改变值
    public override int GetNextState(int stateValue)
    {
        State state = (State)stateValue;

        if (playerChecker.IsColliding()) // 外界干扰的状态改变
        {
            return (int)State.RUN;
        }
        switch (state)
        {
            case State.IDLE:
                if (stateMachine.mStateTime > 2)
                {
                    return (int)State.WALK; // 站立两秒就走路
                }
                break;
            case State.WALK:
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
                if (calmTimer.IsStopped())
                {
                    return (int)State.WALK; // 计时器清空，丢失仇恨
                }
                break;
        }
        return stateValue;
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
        }

    }

    // 代替原有的_PhysicsProcess,被状态机调用
    public override void TickPhysics(int stateValue, float delta)
    {
        State state = (State)stateValue;
        switch (state)
        {
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
                if (playerChecker.IsColliding())
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
}
