using Sandbox;

namespace EasyWeapons.Demo.Entities.Components;

public abstract class SpecificEntityComponent<T, I> : EntityComponent<T> where T : Entity where I : class
{
    public I SpecificEntity { get; private set; } = null!;

    protected override void OnActivate()
    {
        if(Entity is not I)
        {
            Log.Error($"Can't add {GetType().Name} to entity type which is not assignable to {typeof(I).Name}.");
            Entity.Components.Remove(this);
            return;
        }

        SpecificEntity = (Entity as I)!;
    }
}
