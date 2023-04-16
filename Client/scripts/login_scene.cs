using Godot;
using System;
using Nakama;

public partial class login_scene : Control
{
	static private Nakama.Client client;
	static private readonly Godot.Color White = new Godot.Color("#FFFFFF");
	static private readonly Godot.Color Red = new Godot.Color("#FF0000");
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		const string Scheme = "http";
		const string Host = "dan-laptop.penguin-major.ts.net";
		const int Port = 7350;
		const string ServerKey = "defaultkey";
		client = new Nakama.Client(Scheme, Host, Port, ServerKey);
		client.Timeout = 10;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	private void _on_back_button_pressed()
	{
		GetTree().ChangeSceneToFile("res://scenes/main_menu.tscn");
	}
	private async void _on_login_button_pressed()
	{
		var EmailNode = GetNode<LineEdit>("VBoxContainer/EmailBox");
		var PasswordNode = GetNode<LineEdit>("VBoxContainer/PasswordBox");

		string Email = EmailNode.Text;
		string Password = PasswordNode.Text;
		try
		{
			var LoginSession = await client.AuthenticateEmailAsync(Email, Password, create: false);
			GetTree().ChangeSceneToFile("res://scenes/map.tscn");
		}
		catch (ApiResponseException e)
		{
			switch (e.GrpcStatusCode)
			{
				case 16:
					GD.Print("Wrong password!");
					PasswordNode.Set("theme_override_colors/font_color", Red);
					break;
				case 5:
					GD.Print("Not exist account!");
					PasswordNode.Set("theme_override_colors/font_color", Red);
					break;
			}
		}
	}
	private void _on_email_box_text_changed(string new_text)
	{
		GetNode<LineEdit>("VBoxContainer/EmailBox").Set("theme_override_colors/font_color", White);
	}


	private void _on_password_box_text_changed(string new_text)
	{
		GetNode<LineEdit>("VBoxContainer/PasswordBox").Set("theme_override_colors/font_color", White);
	}
}







