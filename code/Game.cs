using EasyWeapons.Demo.Players;
using EasyWeapons.Events;
using Sandbox;

namespace EasyWeapons.Demo;

public partial class MyGame : GameManager
{
    public override void ClientJoined(IClient cl)
    {
        base.ClientJoined(cl);
        var player = new DemoPlayer(cl);
        player.Respawn();

        cl.Pawn = player;
    }

    public override void Simulate(IClient client)
    {
        Event.Run(CustomGameEvent.PreSimulate, client);
        base.Simulate(client);
        Event.Run(CustomGameEvent.PostSimulate, client);
    }
}
