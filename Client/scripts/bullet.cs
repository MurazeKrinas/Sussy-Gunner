using Godot;
using System;

public partial class bullet : CharacterBody2D
{
    float maxRange = 1000;
    float distanceTravelled = 0;
    public float damage = 20;
    float speed = 1200;
    public override void _Process(double delta)
    {
        float moveAmount = (float)(speed * delta);
        Position += Transform.X.Normalized() * (float)moveAmount;
        distanceTravelled += moveAmount;
        if(distanceTravelled > maxRange)
        {
            QueueFree();
        }
    }
    
    public void _on_area_2d_area_entered(Area2D area)
    {
        if (area.Name == "WallArea")
        {
            //GD.Print("HIT");
            QueueFree();
        }
    }
}

