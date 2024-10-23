using Godot;
using System;


public partial class StateMachine : Node
{
	// 当前状态为了和已有状态产生区别，定义为-1
	private int mCurrentState = -1;
	private float mStateTime;
	private MainPlayer mPlayer;

	// 本质上是个getter和setter的集合体
	public int CurrentState
	{
		get => mCurrentState;
		set
		{
			// 调用TransitionState方法来处理状态转换
			mPlayer.TransitionState((State)mCurrentState, (State)value);
			mCurrentState = value;
			mStateTime = 0.0f; // 重置状态时间
		}
	}

	public override void _Ready()
	{
		// 找到父节点，先让父节点初始化
		mPlayer = (MainPlayer)GetParent();
		mPlayer._Ready();
		// 初始化为IDLE状态
		CurrentState = 0;
	}


	public override void _PhysicsProcess(double delta)
	{
		while (true)
		{
			// 获取下一个状态
			int nextState = (int)mPlayer.GetNextState((State)mCurrentState);
			if (mCurrentState == nextState) // 直到状态确定才退出
			{
				break;
			}
			CurrentState = nextState; // 这里setter方法有额外逻辑(TransitionState)
		}
		// 这里状态机的_PhysicsProcess循环代替了Player的循环
		mPlayer.TickPhysics((State)mCurrentState, (float)delta);
	}
}
