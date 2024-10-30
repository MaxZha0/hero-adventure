using Godot;
using System;

public partial class StatsPanel : HBoxContainer
{
	private PlayerStats mPlayerStats;

	private TextureProgressBar mHealthBar;

	private TextureProgressBar mEnergyBar;

	private TextureProgressBar mUnderHealthBar;
	public override void _Ready()
	{
		mPlayerStats = GetNode<Game>("/root/Game").mPlayerStats;
		mHealthBar = GetNode<TextureProgressBar>("Bars/HealthBar");
		mUnderHealthBar = GetNode<TextureProgressBar>("Bars/HealthBar/UnderHealthBar");
		mEnergyBar = GetNode<TextureProgressBar>("Bars/EnergyBar");
		// 动态链接信号（把节点的area_entered信号连接到当前节点的OnAreaEntered方法）
		mPlayerStats.Connect("HealthChange", new Callable(this, "UpdateHealth"));
		// 初次同步
		InitHealth();
		// 能量信号连接
		mPlayerStats.Connect("EnergyChange", new Callable(this, "UpdateEnergy"));
		UpdateEnergy();
	}

	private void InitHealth()
	{
		float current = mPlayerStats.Health / (float)mPlayerStats.MaxHealth;
		mHealthBar.Value = current;
		mUnderHealthBar.Value = current;
	}

	private void UpdateHealth()
	{
		float current = mPlayerStats.Health / (float)mPlayerStats.MaxHealth;
		mHealthBar.Value = current;
		// 创建补间动画，延迟
		CreateTween().TweenProperty(mUnderHealthBar, "value", current, 0.8);
	}

	private void UpdateEnergy()
	{
		float current = mPlayerStats.Energy / (float)mPlayerStats.MaxEnergy;
		mEnergyBar.Value = current;
	}
}
