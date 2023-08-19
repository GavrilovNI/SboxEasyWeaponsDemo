using EasyWeapons.Entities;
using EasyWeapons.Entities.Components;
using Sandbox;
using System;
using System.Collections.Generic;

namespace EasyWeapons.Demo.Entities.Components;

public partial class DefaultPawnController : EntityComponent<Entity>, IPawnController, ISimulatedComponent
{
    public virtual int StepSize => 24;
    public virtual int GroundAngle => 45;
    public virtual int JumpSpeed => 300;
    public virtual float Gravity => 800f;

    protected HashSet<string> ControllerEvents = new(StringComparer.OrdinalIgnoreCase);

    protected bool Grounded => GroundEntity.IsValid();

    public Entity? GroundEntity
    {
        get => Entity.GroundEntity;
        set => Entity.GroundEntity = value;
    }


    public virtual void Simulate(IClient client)
    {
        if(object.ReferenceEquals(Entity.Client, client) == false)
            return;

        if(Entity is not IControllableEntity controllable)
            return;

        ControllerEvents.Clear();

        var movement = controllable.InputDirection.Normal;
        var angles = controllable.ViewAngles.WithPitch(0);
        var moveVector = Rotation.From(angles) * movement * 320f;
        var groundEntity = CheckForGround();

        if(groundEntity.IsValid())
        {
            if(!Grounded)
            {
                Entity.Velocity = Entity.Velocity.WithZ(0);
                AddEvent("grounded");
            }

            Entity.Velocity = Accelerate(Entity.Velocity, moveVector.Normal, moveVector.Length, 200.0f * (Input.Down("run") ? 2.5f : 1f), 7.5f);
            Entity.Velocity = ApplyFriction(Entity.Velocity, 4.0f);
        }
        else
        {
            Entity.Velocity = Accelerate(Entity.Velocity, moveVector.Normal, moveVector.Length, 100, 20f);
            Entity.Velocity += Vector3.Down * Gravity * Time.Delta;
        }

        if(Input.Pressed("jump"))
        {
            DoJump();
        }

        var mh = new MoveHelper(Entity.Position, Entity.Velocity)
        {
            Trace = controllable.CreateTrace()
        };

        if(mh.TryMoveWithStep(Time.Delta, StepSize) > 0)
        {
            if(Grounded)
            {
                mh.Position = StayOnGround(mh.Position);
            }
            Entity.Position = mh.Position;
            Entity.Velocity = mh.Velocity;
        }

        GroundEntity = groundEntity;
    }

    protected virtual void DoJump()
    {
        if(Grounded)
        {
            Entity.Velocity = ApplyJump(Entity.Velocity, "jump");
        }
    }

    protected virtual Entity? CheckForGround()
    {
        var controllable = (Entity as IControllableEntity)!;

        if(Entity.Velocity.z > 100f)
            return null;

        var trace = controllable.CreateTrace(2f).FromTo(Entity.Position, Entity.Position + Vector3.Down).Run();

        if(!trace.Hit)
            return null;

        if(trace.Normal.Angle(Vector3.Up) > GroundAngle)
            return null;

        return trace.Entity;
    }

    protected virtual Vector3 ApplyFriction(Vector3 input, float frictionAmount)
    {
        float StopSpeed = 100.0f;

        var speed = input.Length;
        if(speed < 0.1f)
            return input;

        // Bleed off some speed, but if we have less than the bleed
        // threshold, bleed the threshold amount.
        float control = speed < StopSpeed ? StopSpeed : speed;

        // Add the amount to the drop amount.
        var drop = control * Time.Delta * frictionAmount;

        // scale the velocity
        float newspeed = speed - drop;
        if(newspeed < 0)
            newspeed = 0;
        if(newspeed == speed)
            return input;

        newspeed /= speed;
        input *= newspeed;

        return input;
    }

    protected virtual Vector3 Accelerate(Vector3 input, Vector3 wishdir, float wishspeed, float speedLimit, float acceleration)
    {
        if(speedLimit > 0 && wishspeed > speedLimit)
            wishspeed = speedLimit;

        var currentspeed = input.Dot(wishdir);
        var addspeed = wishspeed - currentspeed;

        if(addspeed <= 0)
            return input;

        var accelspeed = acceleration * Time.Delta * wishspeed;

        if(accelspeed > addspeed)
            accelspeed = addspeed;

        input += wishdir * accelspeed;

        return input;
    }

    protected virtual Vector3 ApplyJump(Vector3 input, string jumpType)
    {
        AddEvent(jumpType);

        return input + Vector3.Up * JumpSpeed;
    }

    protected virtual Vector3 StayOnGround(Vector3 position)
    {
        var controllable = (Entity as IControllableEntity)!;

        var start = position + Vector3.Up * 2;
        var end = position + Vector3.Down * StepSize;

        // See how far up we can go without getting stuck
        var trace = controllable.CreateTrace().FromTo(position, start).Run();
        start = trace.EndPosition;

        // Now trace down from a known safe position
        trace = controllable.CreateTrace().FromTo(start, end).Run();

        if(trace.Fraction <= 0)
            return position;
        if(trace.Fraction >= 1)
            return position;
        if(trace.StartedSolid)
            return position;
        if(Vector3.GetAngle(Vector3.Up, trace.Normal) > GroundAngle)
            return position;

        return trace.EndPosition;
    }

    public virtual bool HasEvent(string eventName)
    {
        return ControllerEvents.Contains(eventName);
    }

    protected virtual void AddEvent(string eventName)
    {
        if(HasEvent(eventName))
            return;

        ControllerEvents.Add(eventName);
    }
}
