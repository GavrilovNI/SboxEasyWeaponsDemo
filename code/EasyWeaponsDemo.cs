using EasyWeapons.Demo.UI;
using EasyWeapons.Events;
using EasyWepons.Demo.Entities.Pawns;
using Sandbox;
using System.Linq;

namespace EasyWeapons.Demo;

public partial class EasyWeaponsDemo : GameManager
{
    public EasyWeaponsDemo()
    {
        if(Game.IsClient)
            Game.RootPanel = new RootUI();
    }

    public override void ClientJoined(IClient client)
    {
        base.ClientJoined(client);
        RespawnPlayer(client);
    }

    public virtual void TeleportToSpawn(Entity entity)
    {
        var spawn = Entity.All.OfType<SpawnPoint>().FirstOrDefault();
        if(spawn is not null)
            entity.Transform = spawn.Transform;
    }

    public virtual void RespawnPlayer(IClient client)
    {
        if(client.Pawn.IsValid())
            client.Pawn.Delete();

        var pawn = new Pawn();
        TeleportToSpawn(pawn);
        client.Pawn = pawn;
        pawn.DressFromClient(client);
    }


    [ClientRpc]
    protected virtual void ResetMapClient()
    {
        ResetMap();
    }

    public virtual void ResetMap()
    {
        Sandbox.Game.ResetMap(Entity.All.Where(x => !DefaultCleanupFilter(x)).ToArray());
        if(Game.IsServer)
            ResetMapClient();
    }

    protected static bool DefaultCleanupFilter(Entity entity)
    {
        // Basic Source engine stuff
        var className = entity.ClassName;
        if(className == "player" || className == "worldent" || className == "worldspawn" || className == "soundent" || className == "player_manager")
        {
            return false;
        }

        // When creating entities we only have classNames to work with..
        // The filtered entities below are created through code at runtime, so we don't want to be deleting them
        if(entity == null || !entity.IsValid)
            return true;

        // Gamemode entity
        if(entity is BaseGameManager)
            return false;

        // HUD entities
        if(entity.GetType().IsBasedOnGenericType(typeof(HudEntity<>)))
            return false;

        // Player related stuff, clothing and weapons
        foreach(var cl in Game.Clients)
        {
            if(entity.Root == cl.Pawn)
                return false;
        }

        // Do not delete view model
        if(entity is BaseViewModel)
            return false;

        return true;
    }

    public override void BuildInput()
    {
        base.BuildInput();
        Event.Run(CustomGameEvent.Client.BuildInput.Post);
    }
}
