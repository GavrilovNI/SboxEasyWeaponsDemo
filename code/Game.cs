using EasyWeapons.Demo.Players;
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

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
}
