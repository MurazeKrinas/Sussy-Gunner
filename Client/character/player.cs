using Godot;
using System;
using System.Threading.Tasks;
using Chickensoft.GoDotNet;
using Nakama;
using Nakama.TinyJson;

public partial class player : CharacterBody2D
{
    double timer = 0.0;
    private ClientNode ClientNode => this.Autoload<ClientNode>();
    private IMatch match;
    public const float Speed = 300.0f;
    private bool isFlip = false;
    private bool SendIdleState = false;
    private int ammoAmount = 20;
    private int health = 10;
    private int DeathCountDown = 500;
    AnimationPlayer animationPlayer;
    Sprite2D body, DeathBody, gun;
    CollisionShape2D colision;
    public void DecHealth() => health -= 2;
    public void SetHealth(int X) => health = X;
    public void SetMatch(IMatch X) => match = X;
    private void LetDead() => GetNode<AnimationPlayer>("Character/Animation").Play("death");
    private void LetLive() => GetNode<AnimationPlayer>("Character/Animation").Play("idle");
    private void LetMove()
    {
        GetNode<AnimationPlayer>("Character/Animation").Play("running");
        MoveAndSlide();
    }
    public override async void _Ready()
    {
        animationPlayer = GetNode<AnimationPlayer>("Character/Animation");
        body = GetNode<Sprite2D>("Character/Body");
        DeathBody = GetNode<Sprite2D>("Character/DeathBody");
        gun = GetNode<Sprite2D>("Character/Hand");
        colision = GetNode<CollisionShape2D>("Collision");
    }
    public override void _PhysicsProcess(double delta)
    {
        if (Name == ClientNode.Session.Username)
        {
            var CheckChangePos = Position;
            var AreaShape = GetNode<CollisionShape2D>($"Player_{ClientNode.Session.UserId}/AreaShape");
            if (health <= 0) //User dead
            {
                if (!DeathBody.Visible) //Still not send match state
                {
                    var opCode = 3; //Send state dead
                    Task.Run(async () => await ClientNode.Socket.SendMatchStateAsync(match.Id, opCode, JsonWriter.ToJson("Dead!")));

                    body.Visible = gun.Visible = false;
                    DeathBody.Visible = colision.Disabled = AreaShape.Disabled = true;
                    LetDead();
                }
                --DeathCountDown;
                if (DeathCountDown <= 0)
                {
                    var opCode = 3; //Send state alive
                    Task.Run(async () => await ClientNode.Socket.SendMatchStateAsync(match.Id, opCode, JsonWriter.ToJson("Live!")));

                    health = 10;
                    DeathCountDown = 500;
                    body.Visible = gun.Visible = true;
                    DeathBody.Visible = colision.Disabled = AreaShape.Disabled = false;
                    LetLive();
                }
            }
            else
            {
                gun.LookAt(GetGlobalMousePosition());
                GetParent().GetNode<Camera2D>("Camera2D").Position = Position;

                var inputDirection = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
                if (Input.IsActionPressed("reload") && ammoAmount == 0)
                    ammoAmount = 20; //Reload bullet
                if (inputDirection.X != 0)
                    body.FlipH = inputDirection.X < 0;
                gun.FlipV = body.FlipH;

                if (inputDirection != Vector2.Zero)
                {
                    SendIdleState = false;
                    Velocity = inputDirection * Speed;
                    LetMove();
                    var opCode = 0; //Send position
                    var state = new ClientNode.PlayerState { isDirection = true, PosX = inputDirection.X, PosY = inputDirection.Y, GunRoate = gun.Rotation, GunFlip = gun.FlipV };
                    Task.Run(async () => await ClientNode.Socket.SendMatchStateAsync(match.Id, opCode, JsonWriter.ToJson(state)));
                }
                else
                {
                    LetLive();
                    if (!SendIdleState)
                    {
                        SendIdleState = true;
                        var opCode = 0; //Send position
                        var state = new ClientNode.PlayerState { isDirection = false, PosX = Position.X, PosY = Position.Y };
                        Task.Run(async () => await ClientNode.Socket.SendMatchStateAsync(match.Id, opCode, JsonWriter.ToJson(state)));
                    }
                }
            }
        }
    }
    public override void _UnhandledInput(InputEvent @event)
    {
        if (Name == ClientNode.Session.Username)
        {
            if (@event is InputEventMouseButton mouseEvent)
            {
                if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed && ammoAmount > 0)
                {
                    Task.Run(async () => await Shoot());

                    var opCode = 2;
                    Task.Run(async () => await ClientNode.Socket.SendMatchStateAsync(match.Id, opCode, JsonWriter.ToJson(gun.Rotation)));
                }
            }
        }
    }
    public async Task Shoot(float? Rotation = null)
    {
        if (health <= 0) return; //User dead cannot shoot

        var bulletPos = GetNode<Marker2D>("Character/Hand/BulletPos");
        var bulletScene = GD.Load<PackedScene>("res://scenes/bullet.tscn");
        var _bullet = bulletScene.Instantiate<bullet>();

        _bullet.Rotation = (float)((Rotation == null) ? gun.Rotation : Rotation);
        _bullet.Position = bulletPos.GlobalPosition;
        _bullet.Scale = new Vector2((float)0.5, (float)0.5);
        _bullet.SetMatch(match);
        ammoAmount -= 1;
        GetParent().AddChild(_bullet);
    }
    public async Task Move(Vector2 Direction, float GunRoate, bool GunFlip, string UserID)
    {
        if (health <= 0) return;

        var UserAreaShape = GetNode<CollisionShape2D>($"Player_{UserID}/AreaShape");
        Velocity = (Vector2)(Direction * Speed);
        gun.Rotation = GunRoate;
        gun.FlipV = body.FlipH = GunFlip;
        CallDeferred("LetMove");
    }
    public async Task Stop() => CallDeferred("LetLive");
    public async Task DeadOrLive(string UserID, bool Live = false)
    {
        var areashape = GetNode<CollisionShape2D>($"Player_{UserID}/AreaShape");

        body.Visible = gun.Visible = Live;
        DeathBody.Visible = colision.Disabled = areashape.Disabled = !Live;
        DeathBody.FlipH = body.FlipH;

        if (Live) CallDeferred("LetLive");
        else CallDeferred("LetDead");
    }
}
