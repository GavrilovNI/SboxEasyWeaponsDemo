using Sandbox;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EasyWeapons.Demo.Commands;

public static partial class CommandUtils
{
    public enum RespondType
    {
        Info,
        Error
    }

    [ClientRpc]
    public static void DrawRespond(RespondType respondType, string info)
    {
        switch(respondType)
        {
            case RespondType.Info:
                Log.Info(info);
                return;
            case RespondType.Error:
                Log.Error(info);
                return;
        }
    }

    public static void SendRespond(RespondType respondType, string info)
    {
        var client = ConsoleSystem.Caller;
        if(client is null)
        {
            switch(respondType)
            {
                case RespondType.Info:
                    Log.Info(info);
                    return;
                case RespondType.Error:
                    Log.Error(info);
                    return;
            }
            return;
        }

        DrawRespond(To.Single(client), respondType, info);
    }

    public static bool TryGetCallerOrError(out IClient? client)
    {
        client = ConsoleSystem.Caller;
        if(client is null)
        {
            SendRespond(RespondType.Error, "Only players can call this command!");
            client = null;
            return false;
        }

        return true;
    }

    public static bool TryGetClientOrError(string name, out IClient? client)
    {
        client = Game.Clients.FirstOrDefault(c => c.Name == name);
        if(client is null)
        {
            SendRespond(RespondType.Error, $"Client {name} not found!");
            client = null;
            return false;
        }

        return true;
    }

    public static bool TryGetPawnOrError<T>(string name, out T? pawn) where T : class, IEntity
    {
        if(TryGetClientOrError(name, out var client) == false)
        {
            pawn = null;
            return false;
        }

        pawn = client!.Pawn as T;
        if(pawn.IsValid())
            return true;

        SendRespond(RespondType.Error, $"Player {name} with pawn of type {typeof(T)} not found!");
        return false;
    }

    public static bool TryGetCallerPawnOrError<T>(out T? pawn) where T : class, IEntity
    {
        if(TryGetCallerOrError(out var client) == false)
        {
            pawn = null;
            return false;
        }

        pawn = client!.Pawn as T;
        if(pawn.IsValid())
            return true;

        SendRespond(RespondType.Error, $"Only players with pawn of type {typeof(T)} can call this command!");
        return false;
    }


    public static bool TryGetCallerPawnComponentOrError<T>(out T? component, bool includeDisabled = false) where T : class, IComponent
    {
        if(TryGetCallerPawnOrError<Entity>(out var pawn) == false)
        {
            component = null;
            return false;
        }

        component = pawn!.Components.Get<T>(includeDisabled);
        if(component is null)
        {
            CommandUtils.SendRespond(RespondType.Error, $"Pawn's component {typeof(T).Name} not found!");
            return false;
        }

        return true;
    }

    public static bool TryGetGameManagerOrError<T>(out T? gameManager) where T : GameManager
    {
        gameManager = GameManager.Current as T;
        if(gameManager.IsValid())
            return true;

        SendRespond(RespondType.Error, $"{nameof(GameManager)} of type {typeof(T)} not found!");
        return false;
    }

    public static async Task<Prop?> SpawnPropOrError(string modelName)
    {
        if(IsPackageIdentifier(modelName))
        {
            var package = await TryFetchPackageWithTypeOrError(modelName, Package.Type.Model);
            if(package is null)
            {
                CommandUtils.SendRespond(RespondType.Error, $"Cloud model not found!");
                return null;
            }
            modelName = await LoadModelFromCloud(package);
        }
        var model = Model.Load(modelName);
        if(model is null)
        {
            CommandUtils.SendRespond(RespondType.Error, $"Can't find model!");
            return null;
        }

        return new Prop() { Model = model };
    }

    public static async Task<Entity?> SpawnEntityOrError(string entityName)
    {
        if(IsPackageIdentifier(entityName))
        {
            var package = await TryFetchPackageWithTypeOrError(entityName, Package.Type.Addon);
            if(package is null)
            {
                CommandUtils.SendRespond(RespondType.Error, $"Cloud entity not found!");
                return null;
            }

            if(package.Tags.Contains("runtime") == false)
            {
                CommandUtils.SendRespond(RespondType.Error, $"Couldn't spawn entity. Package is not a runtime package!");
                return null;
            }
            entityName = await LoadEntityFromCloud(package);
        }

        var entityType = TypeLibrary.GetType<Entity>(entityName)?.TargetType;
        if(entityType is null)
        {
            CommandUtils.SendRespond(RespondType.Error, $"Can't find entity '{entityName}'!");
            return null;
        }

        if(!TypeLibrary.HasAttribute<SpawnableAttribute>(entityType))
        {
            CommandUtils.SendRespond(RespondType.Error, $"Entity '{entityName}' is not spawnable!");
            return null;
        }

        return TypeLibrary.Create<Entity>(entityType);
    }


    public static async Task<string> LoadModelFromCloud(Package package)
    {
        var model = package.GetMeta("PrimaryAsset", "models/dev/error.vmdl");
        await package.MountAsync();
        return model;
    }

    public static async Task<string> LoadEntityFromCloud(Package package)
    {
        var entityName = package.GetMeta("PrimaryAsset", "");
        await package.MountAsync();
        return entityName;
    }

    public static async Task<Package?> TryFetchPackageWithTypeOrError(string modelIdent, Package.Type type)
    {
        var package = await Package.FetchAsync(modelIdent, false);
        if(package == null || package.Revision == null)
        {
            CommandUtils.SendRespond(RespondType.Error, $"{nameof(Package)} not found!");
            return null;
        }

        if(package.PackageType != type)
        {
            CommandUtils.SendRespond(RespondType.Error, $"Wrong {nameof(Package.Type)}, only {Enum.GetName<Package.Type>(type)} is accepatable!");
            return null;
        }

        return package;
    }

    public static bool IsPackageIdentifier(string identifier)
    {
        if(identifier.Count(x => x == '.') != 1)
            return false;

        if(identifier.EndsWith(".vmdl", System.StringComparison.OrdinalIgnoreCase))
            return false;

        if(identifier.EndsWith(".vmdl_c", System.StringComparison.OrdinalIgnoreCase))
            return false;

        return true;
    }
}
