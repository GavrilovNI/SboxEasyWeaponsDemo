using Sandbox;
using System;
using System.Linq;


namespace EasyWeapons.Demo.Inventories;

partial class DemoInventory : BaseInventory
{
    public DemoInventory(Player player) : base(player)
    {
    }

    public override bool CanAdd(Entity entity)
    {
        if(!entity.IsValid())
            return false;

        if(!base.CanAdd(entity))
            return false;

        return !IsCarryingType(entity.GetType());
    }

    public override bool Add(Entity entity, bool makeActive = false)
    {
        if(!entity.IsValid())
            return false;

        if(IsCarryingType(entity.GetType()))
            return false;

        return base.Add(entity, makeActive);
    }

    public bool IsCarryingType(Type t)
    {
        return List.Any(x => x?.GetType() == t);
    }

    public override bool Drop(Entity ent)
    {
        if(!Game.IsServer)
            return false;

        if(!Contains(ent))
            return false;

        if(ent is BaseCarriable bc)
        {
            bc.OnCarryDrop(Owner);
        }

        return ent.Parent == null;
    }

    public virtual void ScrollActiveSlot(int delta, bool allowEmpty = true)
    {
        int slotsCount = Count();
        if(slotsCount == 0)
            return;

        if(allowEmpty)
            slotsCount++;

        int currentSlot = GetActiveSlot();

        int newSlot = currentSlot + delta;

        if(allowEmpty)
            newSlot++;

        newSlot %= slotsCount;
        newSlot += slotsCount;
        newSlot %= slotsCount;

        if(allowEmpty)
            newSlot--;

        SetActiveSlot(newSlot, allowEmpty);
    }
}
