using Sandbox;

namespace EasyWeapons.Demo.Commands;

public static class PlayerCommands
{
    [ConCmd.Admin("kill")]
    public static void Kill()
    {
        if(CommandUtils.TryGetCallerPawnOrError<Entity>(out var pawn) == false)
            return;

        pawn!.TakeDamage(new DamageInfo { Damage = pawn.Health });
    }

    [ConCmd.Server("respawn")]
    public static void Respawn()
    {
        if(CommandUtils.TryGetCallerOrError(out var client) == false)
            return;

        if(CommandUtils.TryGetGameManagerOrError<EasyWeaponsDemo>(out var manager) == false)
            return;

        manager!.RespawnPlayer(client!);
    }
}
