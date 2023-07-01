using EasyWeapons.Demo.Players;
using Sandbox;
using System.Threading.Tasks;

namespace EasyWeapons.Demo.Commands;

public static class PlayerCommands
{

    [ConCmd.Admin("noclip")]
    static void DoPlayerNoclip()
    {
        if(ConsoleSystem.Caller.Pawn is DemoPlayer basePlayer)
        {
            if(basePlayer.DevController is NoclipController)
            {
                basePlayer.DevController = null;
            }
            else
            {
                basePlayer.DevController = new NoclipController();
            }
        }
    }

    [ConCmd.Admin("kill")]
    static void DoPlayerSuicide()
    {
        if(ConsoleSystem.Caller.Pawn is DemoPlayer basePlayer)
        {
            basePlayer.TakeDamage(new DamageInfo { Damage = basePlayer.Health * 99 });
        }
    }

    [ConCmd.Server("respawn")]
    public static async Task Spawn()
    {
        var client = ConsoleSystem.Caller;

        if(client == null)
            return;

        var pawn = client.Pawn;
        if(pawn != null)
            pawn.Delete();

        var player = new DemoPlayer(client);
        player.Respawn();

        client.Pawn = player;
    }
}
