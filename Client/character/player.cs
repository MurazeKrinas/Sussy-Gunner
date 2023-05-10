using Godot;
using System;
using System.Threading.Tasks;
using Chickensoft.GoDotNet;
public partial class player : CharacterBody2D
{
	private ClientNode ClientNode => this.Autoload<ClientNode>();
	public const float Speed = 300.0f;
	private bool isFlip = false;
	private int ammoAmount = 20;
	public override void _PhysicsProcess(double delta)
	{
		if (Name == ClientNode.Session.Username)
		{
			var animationPlayer = GetNode<AnimationPlayer>("Character/RunningAnimation");
			var body = GetNode<Sprite2D>("Character/Body");
			var gun = GetNode<Sprite2D>("Character/Hand");
			gun.LookAt(GetGlobalMousePosition());

			Vector2 inputDirection = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");

			if (Input.IsActionPressed("reload") && ammoAmount == 0)
				ammoAmount = 20; //Reload bullet

			if (inputDirection.X != 0)
				body.FlipH = inputDirection.X < 0;

			gun.Position = body.Position;
			gun.FlipV = body.FlipH;

			if (inputDirection != Vector2.Zero)
				animationPlayer.Play("running");
			else animationPlayer.Play("idle");

			Velocity = inputDirection * Speed;
			GetParent().GetNode<Camera2D>("Camera2D").Position = Position;
			MoveAndSlide();
		}

	}
	public async Task Shoot()
	{
		var bulletPos = GetNode<Marker2D>("Character/Hand/BulletPos");
		var bulletScene = GD.Load<PackedScene>("res://scenes/bullet.tscn");
		var _bullet = bulletScene.Instantiate<bullet>();

		_bullet.Rotation = (GetGlobalMousePosition() - GlobalPosition).Angle();
		_bullet.Position = bulletPos.GlobalPosition;
		_bullet.Scale = new Vector2((float)0.5, (float)0.5);
		ammoAmount -= 1;
		GetParent().AddChild(_bullet);
	}
	public async Task Move(Vector2 positionState)
	{
		Velocity = positionState * Speed;
		GetNode<AnimationPlayer>("Character/RunningAnimation").Play("running");
		MoveAndSlide();
	}
	public override void _UnhandledInput(InputEvent @event)
	{
		if (Name == ClientNode.Session.Username)
		{
			if (@event is InputEventMouseButton mouseEvent)
			{
				if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed && ammoAmount > 0)

					Task.Run(async () => await Shoot());
			}
		}
	}

}
