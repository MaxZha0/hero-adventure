using Godot;
using System;

public partial class MainTitle : Control
{
	private Button mNewGameBtn;
	private Button mLoadGameBtn;
	private VBoxContainer mButtonBox;
	public override void _Ready()
	{
		mNewGameBtn = GetNode<Button>("VBox/NewGame");
		mNewGameBtn.GrabFocus();
		mLoadGameBtn = GetNode<Button>("VBox/LoadGame");
		mLoadGameBtn.Disabled = !Game.IsHasSave();

		mButtonBox = GetNode<VBoxContainer>("VBox");
		// 跟随鼠标
		foreach (Button button in mButtonBox.GetChildren())
		{
			button.Connect(SignalName.MouseEntered, new Callable(button, MethodName.GrabFocus));
		}
	}

	public override void _Process(double delta)
	{
	}

	public void OnNewGame()
	{
		GetNode<Game>("/root/Game").NewGame();
	}

	public void OnLoadGame()
	{
		GetNode<Game>("/root/Game").LoadGame();
	}

	public void OnExitGame()
	{
		GetTree().Quit();
	}
}
