using Godot;
using System;

public partial class SaveStone : Interactable
{

	private AnimationPlayer mAnimationPlayer;

	public override void _Ready()
	{
		base._Ready();
		mAnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
	}

	public override void OnInteract()
	{
		base.OnInteract();
		mAnimationPlayer.Play("activated");
		GetNode<Game>("/root/Game").SaveGame();
	}

}
