using EasyWeapons.Weapons;
using Sandbox;
using System.Linq;

namespace EasyWeapons.Demo.Debugging.Entities;

public class WeaponDebugEntity : Entity
{
    public bool IsEnabled { get; private set; }

    private Arrow? _aimRayArrow;

    public WeaponDebugEntity()
    {
        Game.AssertClient();
    }

    public WeaponDebugEntity(Weapon weapon)
    {
        Parent = weapon;
    }

    public override void Spawn()
    {
        base.Spawn();
        Tags.Add("debug");
    }

    protected virtual void DeleteAllDebugEntities()
    {
        foreach(var entity in Children.Where(c => c.Tags.Has("debug")).ToArray())
            entity.Delete();
    }

    protected virtual bool IsParentValidOrNotify()
    {
        if(Parent.IsValid() && Parent is Weapon)
            return true;
        Log.Warning($"{GetType().Name} should be child of {nameof(Weapon)}");
        return false;
    }

    public virtual void Enable()
    {
        if(IsEnabled || IsParentValidOrNotify() == false)
            return;

        if(_aimRayArrow.IsValid() == false)
        {
            _aimRayArrow = new Arrow
            {
                LocalScale = 0.4f,
                Parent = this
            };
        }

        _aimRayArrow.SetAimFrom(arrow =>
        {
            return (Parent.AimRay.Position, Rotation.LookAt(Parent.AimRay.Forward));
        });

        IsEnabled = true;
        Event.Register(this);
    }

    [GameEvent.Tick.Server]
    protected virtual void Tick()
    {
        if(IsEnabled == false)
            return;

        if(IsParentValidOrNotify() == false)
        {
            Disable();
            return;
        }
    }

    public virtual void Disable()
    {
        if(IsEnabled == false)
            return;

        Event.Unregister(this);

        if(_aimRayArrow.IsValid())
            _aimRayArrow.Disable();

        IsEnabled = false;
    }

    protected override void OnDestroy()
    {
        if(IsEnabled)
            Disable();
        base.OnDestroy();
    }

    [Event.Hotload]
    protected virtual void OnHotload()
    {
        if(Game.IsServer == false)
            return;

        DeleteAllDebugEntities();

        _aimRayArrow = null;

        if(IsEnabled)
        {
            Disable();
            Enable();
        }
    }
}