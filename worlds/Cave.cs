using Godot;
using System;

public partial class Cave : World
{
	public override void InitMap()
	{
		mMap = GetNode<TileMapLayer>("TileMap/Rock");
	}
}
