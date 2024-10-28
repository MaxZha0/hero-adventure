using Godot;
using System;

public partial class EntryPoint : Marker2D
{
	public override void _Ready()
	{
		// 添加到分组
		AddToGroup("entry_points");
	}
}
