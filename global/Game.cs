using Godot;
using System;

public partial class Game : Node
{
	public override void _Ready()
	{
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

		foreach (Marker2D node in tree.GetNodesInGroup("entry_points"))
		{
			// 如果分组中有入参点，则更新位置
			if (node.Name == entryPoints)
			{
				// todo 这里定死类型不知道是否有问题
				((World)tree.CurrentScene).UpdatePlayer(node.GlobalPosition);
				break;
			}
		}
	}
}
