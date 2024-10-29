using Godot;
using System;

public partial class Game : Node
{

	// 玩家状态变成全局的
	public PlayerStats playerStats;
	public override void _Ready()
	{
		playerStats = GetNode<PlayerStats>("PlayerStats");
	}

	public override void _PhysicsProcess(double delta)
	{
	}

	public async void ChangeScene(String path, String entryPoints)
	{
		SceneTree tree = GetTree();
		tree.ChangeSceneToFile(path);
		// Await*******************等待信号！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！
		await ToSignal(tree, SceneTree.SignalName.TreeChanged);

		foreach (EntryPoint node in tree.GetNodesInGroup("entry_points"))
		{
			// 如果分组中有入参点，则更新位置
			if (node.Name == entryPoints)
			{
				// todo 这里定死类型World不知道是否有问题
				((World)tree.CurrentScene).UpdatePlayer(node.GlobalPosition, node.direction);
				break;
			}
		}
	}
}
