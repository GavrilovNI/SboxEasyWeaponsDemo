using EasyWeapons.Demo.Entities.Components.Inventories;
using EasyWeapons.Extensions;
using Sandbox;
using System.Linq;
using System.Threading.Tasks;
using static EasyWeapons.Demo.Commands.CommandUtils;

namespace EasyWeapons.Demo.Commands;

public static class InventoryCommands
{
    [ConCmd.Admin("inv_add_ammo")]
    public static void GetAmmo(string ammoName, int count)
    {
        if(CommandUtils.TryGetCallerPawnOrError<Entity>(out var pawn) == false)
            return;

        var inventory = pawn!.GetOrCreateAmmoInventoryComponent()!.AmmoInventory;

        if(inventory.CanAdd(ammoName, count))
            inventory.Add(ammoName, count);
        else
            CommandUtils.SendRespond(RespondType.Error, "Can't add ammo!");
    }

    [ConCmd.Admin("inv_add_entity")]
    public static async Task AddEntity(string entityName, bool makeActive = true)
    {
        if(CommandUtils.TryGetCallerPawnComponentOrError<EntitiesInventory>(out var inventory) == false)
            return;

        var entity = await CommandUtils.SpawnEntityOrError(entityName);
        if(entity == null)
            return;

        bool added = false;
        if(inventory!.CanAdd(entity))
            added = inventory.Add(entity, makeActive);

        if(added == false)
        {
            CommandUtils.SendRespond(RespondType.Error, "Can't add to player's inventory!");
            entity.Delete();
            return;
        }
    }

    [ConCmd.Admin("inv_print")]
    public static void Print()
    {
        if(CommandUtils.TryGetCallerPawnComponentOrError<EntitiesInventory>(out var inventory) == false)
            return;

        if(inventory!.Any() == false)
            CommandUtils.SendRespond(RespondType.Info, "Inventory is empty.");

        foreach(var entity in inventory!)
            CommandUtils.SendRespond(RespondType.Info, entity.ToString());
    }

    [ConCmd.Admin("inv_clear")]
    public static void Clear()
    {
        if(CommandUtils.TryGetCallerPawnComponentOrError<EntitiesInventory>(out var inventory) == false)
            return;

        inventory!.Clear();
    }
}
