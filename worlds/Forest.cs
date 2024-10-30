using Godot;
using System;

public partial class Forest : World
{
	public override void InitMap()
	{
		mMap = GetNode<TileMapLayer>("TileMap/TileTrunk");
	}
}
