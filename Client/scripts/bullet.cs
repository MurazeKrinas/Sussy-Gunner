using Godot;
using System;
using System.Threading.Tasks;
using Chickensoft.GoDotNet;
using Nakama;
using Nakama.TinyJson;

public partial class bullet : CharacterBody2D
{
	private ClientNode ClientNode => this.Autoload<ClientNode>();
	float maxRange = 1000;
	float distanceTravelled = 0;
	public float damage = 20;
	float speed = 1200;
	private IMatch match;
	public void SetMatch(IMatch X) => match = X;
	public override void _Process(double delta)
	{
		float moveAmount = (float)(speed * delta);
		Position += Transform.X.Normalized() * (float)moveAmount;
		distanceTravelled += moveAmount;
		if (distanceTravelled > maxRange)
			QueueFree();
	}

	public void _on_area_2d_area_entered(Area2D area)
	{
		if (area.Name == "WallArea" || area.Name == "Area")
			QueueFree();
	}
}

