using EasyWeapons.Demo.Entities.Pawns;
using Sandbox;
using System.Linq;
using System.Threading.Tasks;
using static EasyWeapons.Demo.Commands.CommandUtils;

namespace EasyWeapons.Demo.Commands;

public static partial class GameCommands
{
    [ConCmd.Admin("map_reset")]
    public static void RespawnEntities()
    {
        if(CommandUtils.TryGetGameManagerOrError<EasyWeaponsDemo>(out var manager) == false)
            return;

        manager!.ResetMap();
    }

    [ConCmd.Server("spawn_entity")]
    public static async Task SpawnEntity(string entityName)
    {
        if(CommandUtils.TryGetCallerPawnOrError<Entity>(out var pawn) == false)
            return;

        var entity = await CommandUtils.SpawnEntityOrError(entityName);
        if(entity is null)
            return;

        if(TryMoveEntityToSpawnByTraceOrError(entity, pawn!) == false)
        {
            entity.Delete();
            return;
        }
    }

    [ConCmd.Server("spawn_prop")]
    public static async Task SpawnProp(string modelName)
    {
        if(CommandUtils.TryGetCallerPawnOrError<Entity>(out var pawn) == false)
            return;

        var prop = await CommandUtils.SpawnPropOrError(modelName);
        if(prop is null)
            return;

        if(TryMoveEntityToSpawnByTraceOrError(prop, pawn!) == false)
        {
            prop.Delete();
            return;
        }
    }

    private static bool TryMoveEntityToSpawnByTraceOrError(Entity entity, Entity aimer, float maxDistance = 500)
    {
        if(aimer is not DefaultPawn pawn)
        {
            SendRespond(RespondType.Error, "Can't find aim ray!");
            return false;
        }

        var trace = Trace.Ray(pawn.EyePosition, pawn.EyePosition + pawn.EyeRotation.Forward * maxDistance)
                            .UseHitboxes()
                            .Ignore(pawn)
                            .Ignore(entity);

        if(entity is ModelEntity modelEntity)
            trace = trace.Size(modelEntity.Model.Bounds);

        var traceResult = trace.Run();

        if(traceResult.StartedSolid)
        {
            SendRespond(RespondType.Error, "Aiming ray started in solid!");
            return false;
        }

        entity.Position = traceResult.EndPosition;
        return true;
    }
}
