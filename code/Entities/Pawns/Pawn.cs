using EasyWeapons.Demo.Entities.Components.Inventories;
using EasyWeapons.Demo.Entities.Pawns;
using EasyWeapons.Recoiles;
using EasyWepons.Demo.Entities.Components;
using Sandbox;

namespace EasyWepons.Demo.Entities.Pawns;

public partial class Pawn : DefaultPawn
{
    [Net, Local]
    public float MaxHealth { get; set; } = 100;

    [BindComponent]
    public EntitiesInventory? EntitiesInventory { get; }

    public override void OnKilled()
    {
        if(LifeState == LifeState.Alive)
            LifeState = LifeState.Dead;
    }

    public override void Spawn()
    {
        Components.Create<EntitiesInventory>();
        Components.Create<DefaultPlayerRecoilApplier>();
        base.Spawn();
    }

    public override void Respawn()
    {
        base.Respawn();

        LifeState = LifeState.Alive;
        Health = MaxHealth;
        MaxHealth = 100;

        LocalScale = 1;
        RenderColor = Color.White;

        Model = DefaultModel;
        SetupPhysicsFromAABB(PhysicsMotionType.Keyframed, Hull.Mins, Hull.Maxs);

        if(Client is not null)
            DressFromClient(Client);

        var animator = Components.Get<PawnAnimator>(true);
        if(animator is not null)
            animator.Enabled = true;
    }

    public override void OnChildRemoved(Entity child)
    {
        base.OnChildRemoved(child);

        if(Game.IsServer)
            EntitiesInventory?.Remove(child);
    }

    public override void SetHull(BBox bBox)
    {
        base.SetHull(bBox);
        SetupPhysicsFromAABB(PhysicsMotionType.Keyframed, Hull.Mins, Hull.Maxs);
    }
}
