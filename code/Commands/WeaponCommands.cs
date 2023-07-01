using EasyWeapons.Weapons.Modules.Attack;
using Sandbox;

namespace EasyWeapons.Demo.Commands;

public static class WeaponCommands
{
    [ConCmd.Admin("weapon_attack")]
    public static void Create(int entityIndex)
    {
        var owner = ConsoleSystem.Caller?.Pawn as Player;

        if(ConsoleSystem.Caller is null)
            return;

        var weapon = Entity.FindByIndex(entityIndex);
        if(weapon is null)
        {
            Log.Error($"Entity with network index {entityIndex} not found");
            return;
        }

        var attackModule = weapon.Components.Get<AttackModule>(true);

        if(attackModule is null)
        {
            Log.Error($"Attack module not found");
            return;
        }


        if(attackModule.CanStartAttack() == false)
        {
            Log.Error($"Attack can't be started");
            return;
        }

        attackModule.Attack();
    }
}
