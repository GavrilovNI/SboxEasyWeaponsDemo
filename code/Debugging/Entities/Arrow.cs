using Sandbox;
using System;

namespace EasyWeapons.Demo.Debugging.Entities;

public class Arrow : ModelEntity
{
    public const float ModelLength = 48f;
    private Func<Arrow, (Vector3, Rotation)>? _aimer = null;

    public override void Spawn()
    {
        base.Spawn();
        SetModel("models/arrow.vmdl");
        EnableAllCollisions = false;
        EnableHideInFirstPerson = true;
        EnableDrawing = true;
        EnableHitboxes = false;
        Tags.Add("debug");
    }

    public void SetAimAt(Func<Arrow, (Vector3, Rotation)> aimer)
    {
        _aimer = aimer;
        Enable();
    }

    public void SetAimFrom(Func<Arrow, (Vector3, Rotation)> aimer)
    {
        _aimer = (Arrow arrow) =>
        {
            (var position, var rotation) = aimer(arrow);
            position += arrow.Rotation.Down * ModelLength * Scale;
            return (position, rotation);
        };
        Enable();
    }

    protected virtual void Enable()
    {
        EnableDrawing = true;
        Event.Register(this);
    }

    public virtual void Disable()
    {
        _aimer = null;
        EnableDrawing = false;
        Event.Unregister(this);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        Event.Unregister(this);
    }

    [GameEvent.Tick.Server]
    protected virtual void Tick()
    {
        if(EnableDrawing == false)
            return;

        if(_aimer != null)
        {
            (Position, var rotation) = _aimer(this);
            Rotation = rotation * Rotation.FromPitch(-90);
        }
    }
}