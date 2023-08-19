using EasyWeapons.Weapons;
using EasyWeapons.Weapons.Modules;
using EasyWeapons.Weapons.Modules.Attack;
using static EasyWeapons.Demo.Commands.CommandUtils;
using Sandbox;

namespace EasyWeapons.Demo.Commands;

public static class WeaponCommands
{
    [ConCmd.Admin("weapon_attack")]
    public static void Create(int entityIndex)
    {
        if(TryGetWeaponModuleOrError<AttackModule>(entityIndex, out var attackModule, true) == false)
            return;

        if(attackModule!.CanStartAttack() == false)
        {
            CommandUtils.SendRespond(RespondType.Error, "Attack can't be started!");
            return;
        }

        attackModule.Attack();
    }



    public static bool TryGetWeaponOrError<T>(int entityIndex, out T? weapon) where T : Weapon
    {
        weapon = Entity.FindByIndex(entityIndex) as T;
        if(weapon is null)
        {
            CommandUtils.SendRespond(RespondType.Error, $"Can't find weapon with type '{typeof(T).Name}' and index {entityIndex}!");
            return false;
        }

        return true;
    }

    public static bool TryGetWeaponModuleOrError<T>(int entityIndex, out T? module, bool includeDisabled = false) where T : WeaponModule
    {
        if(TryGetWeaponOrError<Weapon>(entityIndex, out var weapon) == false)
        {
            module = null;
            return false;
        }

        module = weapon!.Components.Get<T>(includeDisabled);
        if(module is null)
        {
            CommandUtils.SendRespond(RespondType.Error, $"Can't find module of type '{typeof(T).Name}'!");
            return false;
        }

        return true;
    }
}
