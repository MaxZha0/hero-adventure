using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class World : Node
{

	protected TileMapLayer mMap;

	protected Camera2D mCamera;

	protected MainPlayer mPlayer;

	// 每个场景初始化自己的地图
	public virtual void InitMap()
	{

	}

	public override void _Ready()
	{
		InitMap();
		mCamera = GetNode<Camera2D>("Player/Camera2D");
		mPlayer = GetNode<MainPlayer>("Player");

		Rect2I usedRect = mMap.GetUsedRect().Grow(-1);
		Vector2I tileSize = mMap.TileSet.TileSize;

		// usedRect包含两点，坐上点与右下点，包含含义是格块，再乘以单个块的长度即可计算相机极限
		mCamera.LimitLeft = usedRect.Position.X * tileSize.X;
		mCamera.LimitRight = usedRect.End.X * tileSize.X;
		mCamera.LimitTop = usedRect.Position.Y * tileSize.Y;
		mCamera.LimitBottom = usedRect.End.Y * tileSize.Y;
		mCamera.ResetSmoothing();
	}

	// 更新玩家位置 和 面部朝向
	public void UpdatePlayer(Vector2 pos, MainPlayer.FaceDirections face)
	{
		mPlayer.GlobalPosition = pos;
		mPlayer.FaceDirection = face;
		// 镜头跟随
		mCamera.ResetSmoothing();
		// mCamera.ForceUpdateScroll();
	}

	public Godot.Collections.Dictionary toDict()
	{
		List<NodePath> enemiesAlive = new();
		foreach (Enemy enemy in GetTree().GetNodesInGroup("enemy"))
		{
			var path = GetPathTo(enemy);
			enemiesAlive.Add(path);
		}
		return new Dictionary("", enemiesAlive);

	}

	public void fromDict(Dictionary dic)
	{

	}
}
