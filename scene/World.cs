using Godot;
using System;

public partial class World : Node2D
{
	private TileMapLayer mMap;

	private Camera2D mCamera;

	private MainPlayer mPlayer;

	public override void _Ready()
	{
		mCamera = GetNode<Camera2D>("Player/Camera2D");
		mMap = GetNode<TileMapLayer>("TileMap/TileTrunk");
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

	public override void _Process(double delta)
	{
	}

	// 更新玩家位置
	public void UpdatePlayer(Vector2 pos)
	{
		mPlayer.GlobalPosition = pos;
		// 镜头跟随
		mCamera.ResetSmoothing();
		// mCamera.ForceUpdateScroll();
	}
}
