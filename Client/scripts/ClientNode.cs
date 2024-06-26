using System;
using System.Collections.Generic;
using Godot;
using Nakama;
public partial class ClientNode : Node
{
    [Serializable]
    public class PlayerState
    {
        public bool isHost = false, isFireBullet = false, isIceBullet = false, isDirection = false;
        public int Health = 10, Kill = 0, Dead = 0;
        public float PosX = 0, PosY = 0;
        public float GunRoate = 0;
        public bool GunFlip = false;
    }
    public bool isHost = false;
    private const string Scheme = "https";
    private const string Host = "sussy-gunner.fly.dev";
    private const int Port = 443;
    private const string ServerKey = "defaultkey";
    public IClient? Client;
    public ISocket? Socket;
    public ISession? Session;

    public override void _Ready()
    {
        Client = new Nakama.Client(Scheme, Host, Port, ServerKey);
        Client.Timeout = 10;
        Socket = Nakama.Socket.From(Client);
    }

}
