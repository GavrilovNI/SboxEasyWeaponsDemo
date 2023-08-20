using EasyWeapons.Entities.Components;
using EasyWeapons.Events;
using EasyWepons.Demo.Entities.Components;
using Sandbox;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EasyWeapons.Demo.Entities.Components.Inventories;

public partial class EntitiesInventory : EntityComponent, IEnumerable<Entity>, IResetableComponent, ISimulatedComponent, IFrameSimulatedComponent, ICitizenAnimator
{
    [Net]
    protected IList<Entity> Entities { get; set; } = new List<Entity>();

    [Net, Change]
    public Entity? Active { get; private set; }

    protected bool DeployNeeded { get; set; } = false;


    public virtual int Count => Entities.Count;



    public virtual bool Contains(Entity ent) => Entities.Contains(ent);

    public virtual bool CanAdd(Entity entity)
    {
        if(entity.IsValid() == false)
            return false;

        if(Contains(entity))
            return false;

        if(entity is BaseCarriable carriable)
            return carriable.CanCarry(Entity);

        return true;
    }

    public virtual bool Add(Entity entity, bool makeActive = false)
    {
        Game.AssertServer();

        if(entity.Owner != null)
            return false;

        if(CanAdd(entity) == false)
            return false;

        if(entity is BaseCarriable carriable)
            carriable.OnCarryStart(Entity);

        Entities.Add(entity);
        entity.SetParent(Entity, true);

        if(makeActive)
            SetActive(entity);

        return true;
    }

    public virtual bool Remove(Entity entity, bool dropped = false)
    {
        Game.AssertServer();

        if(Contains(entity))
        {
            if(dropped)
            {
                if(entity is BaseCarriable carriable)
                    carriable.OnCarryDrop(Entity);
            }

            if(object.ReferenceEquals(Active, entity))
                SetActive(null);
            Entities.Remove(entity);

            return true;
        }
        return false;
    }

    public virtual Entity? Get(int slot)
    {
        if(slot < 0 || slot >= Entities.Count)
            return null;

        return Entities[slot];
    }

    public virtual bool Drop(Entity entity)
    {
        if(Game.IsServer == false)
            return false;

        if(Contains(entity) == false)
            return false;

        if(ReferenceEquals(Active, entity))
            SetActive(null);

        entity.Parent = null;

        if(entity is BaseCarriable carriable)
            carriable.OnCarryDrop(Entity);

        return true;
    }

    public virtual void Clear()
    {
        Game.AssertServer();

        foreach(var entity in Entities.ToArray())
            entity.Delete();

        Entities.Clear();
        SetActive(null);
    }

    public virtual void Reset() => Clear();

    public virtual int GetActiveSlot()
    {
        if(Active is null)
            return -1;

        return Entities.IndexOf(Active);
    }

    public virtual bool SetActiveSlot(int slot, bool evenIfEmpty = false)
    {
        var entity = Get(slot);
        if(Active == entity)
            return false;

        if(evenIfEmpty == false && entity == null)
            return false;

        SetActive(entity);
        return entity.IsValid();
    }

    public virtual bool SetActive(Entity? entity)
    {
        if(Active == entity)
            return true;

        if(entity is not null && Contains(entity) == false)
            return false;

        var oldActive = Active;
        Active = entity;
        if(Game.IsServer)
            OnActiveChanged(oldActive, Active);

        return true;
    }

    [CustomGameEvent.Client.BuildInput.Post]
    protected virtual void BuildInput()
    {
        if(Active.IsValid() && Contains(Active))
            Active.BuildInput();
    }

    protected virtual void OnActiveChanged(Entity? oldActive, Entity? newActive)
    {
        if(oldActive.IsValid())
        {
            if(oldActive is BaseCarriable carriable)
                carriable.ActiveEnd(oldActive, false);
        }

        bool isNewValid = newActive.IsValid();
        if(isNewValid)
        {
            newActive!.EnableDrawing = true;
            if(newActive is BaseCarriable carriable)
                carriable.ActiveStart(newActive);
        }
        DeployNeeded = isNewValid;
    }

    public void Simulate(IClient client)
    {
        if(Active.IsValid() && Contains(Active))
            Active.Simulate(client);
    }

    public void FrameSimulate(IClient client)
    {
        if(Active.IsValid() && Contains(Active))
            Active.FrameSimulate(client);
    }

    public void Animate(CitizenAnimationHelper helper)
    {
        if(DeployNeeded)
        {
            helper.TriggerDeploy();
            DeployNeeded = false;
        }

        if(Active is BaseCarriable carriable)
            carriable.SimulateAnimator(helper);
    }

    public virtual IEnumerator<Entity> GetEnumerator() => Entities.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => (Entities as IEnumerable).GetEnumerator();
}
