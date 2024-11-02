using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;

public partial class Game : Node
{
	// 转场动画时间
	private static readonly float CHANGE_FLASH_TIME = 0.3f;
	// 所有平台确定都有的目录
	private static readonly string SAVE_PATH = "user://data.sav";
	// 玩家状态变成全局的
	public PlayerStats mPlayerStats;
	// 转场动画黑幕
	private ColorRect mFlashRect;
	// 世界状态: 场景的名称 =>{
	// 	  enemies_alive => {敌人的路径}
	// }
	private Dictionary mWorldState = new();
	public override void _Ready()
	{
		mPlayerStats = GetNode<PlayerStats>("PlayerStats");
		mFlashRect = GetNode<ColorRect>("FlashLayer/FlashRect");

		// 确保一开始为透明
		Color color = mFlashRect.Color;
		color.A = 0;
		mFlashRect.Color = color;
	}

	public async void ChangeScene(String path, Dictionary pos)
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
			((World)tree.CurrentScene).FromDict((Dictionary)mWorldState[newdWorld]);

		}

		GD.Print("change " + pos);
		// 根据传入的值的类型判断。
		if (pos.ContainsKey("EntryPoint")) // 传送门场景
		{
			GD.Print("传送门场景");
			string entryPoints = (string)pos["EntryPoint"];
			foreach (EntryPoint node in tree.GetNodesInGroup("entry_points"))
			{
				// 如果分组中有入参点，则更新位置
				if (node.Name == entryPoints)
				{
					((World)tree.CurrentScene).UpdatePlayer(node.GlobalPosition, node.direction);
					break;
				}
			}
		}
		else if (pos.ContainsKey("Direction") && pos.ContainsKey("PositionX")) // 恢复场景
		{
			GD.Print("恢复场景");
			Vector2 playerPos = new Vector2((float)pos["PositionX"], (float)pos["PositionY"]);
			((World)tree.CurrentScene).UpdatePlayer(playerPos, (MainPlayer.FaceDirections)(float)pos["Direction"]);
		}


		// 入场动画####################################################
		tween = CreateTween();
		// flashRect的透明度在0.2s内变成1
		tween.TweenProperty(mFlashRect, "color:a", 0, CHANGE_FLASH_TIME);
		tree.Paused = false;
	}

	public void SaveGame()
	{
		// 先记录一下世界场景
		SceneTree tree = GetTree();
		World currentWorld = ((World)tree.CurrentScene);
		string oldWorld = tree.CurrentScene.SceneFilePath.GetFile().GetBaseName();
		mWorldState[oldWorld] = currentWorld.ToDict();

		Dictionary data = new()
		{
			{"WorldState",mWorldState},
			{"PlayerState",mPlayerStats.ToDict()},
			{"Scene",currentWorld.SceneFilePath},
			{"Player",new Dictionary(){
				{"Direction", (int)currentWorld.mPlayer.FaceDirection},
				{"PositionX", currentWorld.mPlayer.GlobalPosition.X},
				{"PositionY", currentWorld.mPlayer.GlobalPosition.Y},
				}
			},
		};

		string json = Json.Stringify(data);
		var file = FileAccess.Open(SAVE_PATH, FileAccess.ModeFlags.Write);

		if (file == null)
		{
			return;
		}
		file.StoreString(json);
		file.Close();
	}

	public void LoadGame()
	{
		var file = FileAccess.Open(SAVE_PATH, FileAccess.ModeFlags.Read);
		if (file == null)
		{
			return;
		}
		string json = file.GetAsText();

		Dictionary data = (Dictionary)Json.ParseString(json);

		if (data.ContainsKey("WorldState"))
		{
			mWorldState = (Dictionary)data["WorldState"];
		}
		if (data.ContainsKey("PlayerState"))
		{
			mPlayerStats.FromDict((Dictionary)data["PlayerState"]);
		}
		if (data.ContainsKey("Scene") && data.ContainsKey("Player"))
		{
			Dictionary playerPos = (Dictionary)data["Player"];
			ChangeScene((string)data["Scene"], playerPos);
		}
		// GD.Print("load " + json);
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		base._UnhandledInput(@event);
		if (@event.IsActionPressed("save"))
		{
			SaveGame();
		}

		if (@event.IsActionPressed("load"))
		{
			LoadGame();
		}
	}
}
