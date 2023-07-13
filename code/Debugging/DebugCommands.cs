using EasyWeapons.Demo.Debugging.Entities;
using EasyWeapons.Weapons;
using Sandbox;
using System.Linq;

namespace EasyWeapons.Demo.Debugging;

public static class DebugCommands
{
    [ConCmd.Admin("weapon_debug")]
    public static void DebugWeapon(int weaponEntityId)
    {
        var weapon = Entity.FindByIndex<Weapon>(weaponEntityId);
        if(weapon.IsValid() == false)
        {
            Log.Info($"Weapon not found");
            return;
        }

        var debugEntity = weapon.Children.Where(c => c is WeaponDebugEntity).FirstOrDefault() as WeaponDebugEntity ?? new WeaponDebugEntity(weapon);

        if(debugEntity.IsEnabled)
            debugEntity.Disable();
        else
            debugEntity.Enable();
    }
}
