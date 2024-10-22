using Godot;
using System;


public partial class StateMachine : Node
{
	public static int KEEP_CURRENT = -1;

	private int mCurrentState = KEEP_CURRENT;
	private float mStateTime;

	MainPlayer mPlayer;

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
		mPlayer = (MainPlayer)GetParent();
		mPlayer._Ready();
		this.CurrentState = 0;

	}


	public override void _PhysicsProcess(double delta)
	{
		while (true)
		{
			GD.Print("mCurrentState" + mCurrentState);
			int nextState = (int)mPlayer.GetNextState((State)mCurrentState);
			GD.Print("nextState" + nextState);
			if (mCurrentState == nextState)
			{
				GD.Print(" == ");
				break;
			}

			this.CurrentState = nextState;

			mPlayer.TickPhysics((State)mCurrentState, (float)delta);
		}
	}
}
