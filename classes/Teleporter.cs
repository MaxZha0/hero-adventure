using Godot;
using Godot.Collections;
using System;

public partial class Teleporter : Interactable
{
    // 想要传送的地图
    [Export(PropertyHint.File, "*.tscn")] public String Path;

    // 想要传送的入口点
    [Export] public String EntryPoint;
    public override void OnInteract()
    {
        base.OnInteract();
        // 场景变化是延迟的，调用之后无法立刻切换
        GetNode<Game>("/root/Game").ChangeScene(Path, new Dictionary() { { "EntryPoint", EntryPoint } });
    }
}
