using EasyWeapons.Demo.Entities;
using EasyWeapons.Demo.Entities.Components;
using EasyWeapons.Demo.Math;
using EasyWeapons.Entities.Components;
using EasyWepons.Demo.Entities.Components;
using Sandbox;
using System;

namespace PropHunt.Entities.Components;

public partial class HullUpdater : SpecificEntityComponent<Entity, IHullOwner>, ISimulatedComponent, IResetableComponent
{
    [Net, Local]
    public BBox Default { get; set; } = new
    (
        new Vector3(-16, -16, 0),
        new Vector3(16, 16, 72)
    );

    [Net, Predicted, Local]
    public BBox Target { get; protected set; }

    [Net, Predicted, Local]
    public bool ReachedTarget { get; set; }


    public void Simulate(IClient client)
    {
        if(ReachedTarget == false)
        {
            if(SpecificEntity.Hull.Size.AlmostEqual(Target.Size))
                ReachedTarget = true;
            else
                Update();
        }
    }

    public virtual void Reset() => SetImmediately(Default * Entity.Scale);

    public virtual void SetTarget(BBox newHull)
    {
        Target = newHull;
        ReachedTarget = false;
    }

    public virtual void SetImmediately(BBox newHull)
    {
        SetTarget(newHull);
        SpecificEntity.SetHull(newHull);
        ReachedTarget = true;
    }

    protected virtual void Update()
    {
        var trace = Trace.Ray(0, 0).WithAnyTags(new string[] { "solid", "playerclip", "passbullets", "player" }).Ignore(Entity);

        BBox newHull = SpecificEntity.Hull;
        Vector3 newPosition = Entity.Position;

        foreach(var axisDirection in Enum.GetValues<AxisDirection>())
        {
            foreach(var axis in Enum.GetValues<Axis>())
                UpdateHull(trace, axis, axisDirection, ref newHull, ref newPosition, true);
        }

        SpecificEntity.SetHull(newHull);
        Entity.Position = newPosition;
        ReachedTarget = SpecificEntity.Hull.Mins.AlmostEqual(Target.Mins) && SpecificEntity.Hull.Maxs.AlmostEqual(Target.Maxs);
    }

    private void UpdateHull(Trace trace, Axis axis, AxisDirection axisDirection, ref BBox hull, ref Vector3 position, bool canMove = false)
    {
        var currentValue = SpecificEntity.Hull.GetLimits(axisDirection).GetComponent(axis);
        var targetValue = Target.GetLimits(axisDirection).GetComponent(axis);
        var maxDelta = targetValue - currentValue;

        if(maxDelta.AlmostEqual(0) == false)
        {
            var currentOpposite = SpecificEntity.Hull.GetLimits(axisDirection.GetOpposite()).GetComponent(axis);

            var axisDirectionNormal = axisDirection.GetNormal();

            float newValue;
            if(maxDelta / axisDirectionNormal < 0)
            {
                if(axisDirection == AxisDirection.Positive)
                    newValue = MathF.Max(currentValue + maxDelta, currentOpposite);
                else
                    newValue = MathF.Min(currentValue + maxDelta, currentOpposite);
            }
            else
            {
                var positiveDirection = axis.GetDirection(AxisDirection.Positive).GetNormal();

                var traceHull = hull;
                traceHull.Maxs = traceHull.Maxs.WithComponent(axis, 0);
                traceHull.Mins = traceHull.Mins.WithComponent(axis, 0);
                trace = trace.Size(traceHull);

                var delta = axisDirectionNormal * trace.FromTo(Entity.Position + positiveDirection * currentValue, Entity.Position + positiveDirection * targetValue).Run().Distance;

                if(canMove)
                {
                    if(MathF.Abs(delta) < MathF.Abs(maxDelta))
                    {
                        var neededDelta = maxDelta - delta;
                        var moveDistance = trace.FromTo(Entity.Position + positiveDirection * currentOpposite, Entity.Position + positiveDirection * (currentOpposite - neededDelta)).Run().Distance;

                        var additionalDelta = axisDirectionNormal * moveDistance;

                        position -= positiveDirection * additionalDelta;
                        delta += additionalDelta;
                    }
                }

                newValue = currentValue + delta;
            }

            var limits = hull.GetLimits(axisDirection);
            axis.SetOn(ref limits, newValue);
            axisDirection.SetLimitsOn(ref hull, limits);
        }
    }
}
