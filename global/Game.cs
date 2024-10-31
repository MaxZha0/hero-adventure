using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Game : Node
{
	// 转场动画时间
	private static readonly float CHANGE_FLASH_TIME = 0.3f;
	// 玩家状态变成全局的
	public PlayerStats mPlayerStats;
	// 转场动画黑幕
	private ColorRect mFlashRect;
	// 世界状态: 场景的名称 =>{
	// 	  enemies_alive => {敌人的路径}
	// }
	private System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, List<NodePath>>> mWorldState = new();
	public override void _Ready()
	{
		mPlayerStats = GetNode<PlayerStats>("PlayerStats");
		mFlashRect = GetNode<ColorRect>("FlashLayer/FlashRect");

		// 确保一开始为透明
		Color color = mFlashRect.Color;
		color.A = 0;
		mFlashRect.Color = color;
	}

	public async void ChangeScene(String path, String entryPoints)
	{
		SceneTree tree = GetTree();
		// 切换场景时全场冻结
		tree.Paused = true;

		// 出场动画####################################################
		Tween tween = CreateTween();
		// 转场动画不受暂停影响
		tween.SetPauseMode(Tween.TweenPauseMode.Process);
		// flashRect的透明度在0.2s内变成1
		tween.TweenProperty(mFlashRect, "color:a", 1, CHANGE_FLASH_TIME);
		// 等待完成，再切换场景
		await ToSignal(tween, Tween.SignalName.Finished);


		// 获得旧场景文件的文件名，例如“Cave” ##########################################
		string oldWorld = tree.CurrentScene.SceneFilePath.GetFile().GetBaseName();
		mWorldState[oldWorld] = ((World)tree.CurrentScene).ToDict();


		tree.ChangeSceneToFile(path);
		// Await*******************等待信号！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！
		await ToSignal(tree, SceneTree.SignalName.TreeChanged);

		// 反向操作，将数据恢复到新场景 ##########################################
		string newdWorld = tree.CurrentScene.SceneFilePath.GetFile().GetBaseName();
		if (mWorldState.ContainsKey(newdWorld))
		{
			((World)tree.CurrentScene).FromDict(mWorldState[newdWorld]);

		}

		foreach (EntryPoint node in tree.GetNodesInGroup("entry_points"))
		{
			// 如果分组中有入参点，则更新位置
			if (node.Name == entryPoints)
			{
				((World)tree.CurrentScene).UpdatePlayer(node.GlobalPosition, node.direction);
				break;
			}
		}

		// 入场动画####################################################
		tween = CreateTween();
		// flashRect的透明度在0.2s内变成1
		tween.TweenProperty(mFlashRect, "color:a", 0, CHANGE_FLASH_TIME);
		tree.Paused = false;
	}
}
