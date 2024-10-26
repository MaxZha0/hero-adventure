using System;
using Godot;


public partial class StateMachine : Node
{
    public static int KEEP_CURRENT = -1;
    public float mStateTime;
    // 当前状态为了和已有状态产生区别，定义为-1
    private int mCurrentState = -1;
    private Entity mPlayer;

    // 本质上是个getter和setter的集合体
    public int CurrentState
    {
        get => mCurrentState;
        set
        {
            // 调用TransitionState方法来处理状态转换
            mPlayer.TransitionState(mCurrentState, value);
            mCurrentState = value;
            mStateTime = 0.0f; // 重置状态时间
        }
    }

    public override void _Ready()
    {
        // 找到父节点，先让父节点初始化
        mPlayer = (Entity)GetParent();
        mPlayer._Ready();
        CurrentState = 0; // 初始化为IDLE状态
    }

    public override void _PhysicsProcess(double delta)
    {
        while (true)
        {
            // 获取下一个状态
            int nextState = (int)mPlayer.GetNextState(mCurrentState);
            if (nextState == KEEP_CURRENT) // 直到状态确定才退出
            {
                break;
            }
            CurrentState = nextState; // 这里setter方法有额外逻辑(TransitionState)
        }
        // 这里状态机的_PhysicsProcess循环代替了Player的循环
        mPlayer.TickPhysics(mCurrentState, (float)delta);
        mStateTime += (float)delta;
    }
}
