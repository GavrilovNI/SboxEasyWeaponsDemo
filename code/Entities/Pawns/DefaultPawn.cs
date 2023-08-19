using EasyWeapons.Demo.Entities.Components;
using EasyWeapons.Entities;
using EasyWeapons.Entities.Components;
using EasyWepons.Demo.Entities.Components;
using PropHunt.Entities.Components;
using Sandbox;
using System.Collections.Generic;
using System.ComponentModel;

namespace EasyWeapons.Demo.Entities.Pawns;

public partial class DefaultPawn : AnimatedEntity, IRespawnable, IControllableEntity, IAnimatableCitizen, IPhysicsExpandable, IHullOwner
{
    [Net, Local]
    public Model DefaultModel { get; set; } = Model.Load("models/citizen/citizen.vmdl");

    [ClientInput]
    public Vector3 InputDirection { get; set; }

    [ClientInput]
    public Angles ViewAngles { get; set; }

    [Net, Predicted, Local]
    public BBox Hull { get; set; } = new(new Vector3(-16, -16, 0), new Vector3(16, 16, 72));

    [Browsable(false)]
    public Vector3 EyePosition
    {
        get => CameraSimulator!.EyePosition;
        set => CameraSimulator!.EyePosition = value;
    }

    [Browsable(false)]
    public Vector3 EyeLocalPosition => CameraSimulator!.EyeLocalPosition;

    [Browsable(false)]
    public Rotation EyeRotation
    {
        get => CameraSimulator!.EyeRotation;
        set => CameraSimulator!.EyeRotation = value;
    }

    [Browsable(false)]
    public Rotation EyeLocalRotation => CameraSimulator!.EyeLocalRotation;


    [BindComponent]
    public IPawnController? Controller { get; }

    [BindComponent]
    public PawnAnimator? Animator { get; }

    [BindComponent]
    public CameraSimulator? CameraSimulator { get; }

    [BindComponent]
    public HullUpdater? HullUpdater { get; }

    public override Ray AimRay => CameraSimulator!.AimRay;

    public Ray EyeLookingRay => new(EyePosition, EyeRotation.Forward);


    public override void Spawn()
    {
        Components.Create<HullUpdater>();
        Components.Create<CameraSimulator>();
        Components.Create<DefaultPawnController>();
        Components.Create<PawnAnimator>();
        Respawn();
    }

    public virtual void Respawn()
    {
        Model = DefaultModel;
        EnableDrawing = true;
        EnableHideInFirstPerson = true;
        EnableShadowInFirstPerson = true;

        if(Client is not null)
        {
            Undress();
            DressFromClient(Client);
        }

        foreach(var resetable in Components.GetAll<IResetableComponent>(true))
            resetable.Reset();
    }

    public virtual void DressFromClient(IClient cl)
    {
        var c = new ClothingContainer();
        c.LoadFromClient(cl);
        c.DressEntity(this);
    }

    public virtual void Undress()
    {
        List<Entity> entitiesToremove = new();
        foreach(var child in Children)
        {
            if(child.Tags.Has("clothes"))
            {
                bool canDelete = Game.IsServer || child.IsClientOnly;
                if(canDelete)
                    entitiesToremove.Add(child);
            }
        }
        entitiesToremove.ForEach(e => e.Delete());
    }

    public override void Simulate(IClient client)
    {
        SimulateRotation();

        if(Input.Pressed("view"))
            CameraSimulator?.ChangeView();

        foreach(var simulated in Components.GetAll<ISimulatedComponent>())
            simulated.Simulate(client);
    }

    public override void BuildInput()
    {
        InputDirection = Input.AnalogMove;

        if(Input.StopProcessing)
            return;

        var look = Input.AnalogLook;

        if(ViewAngles.pitch > 90f || ViewAngles.pitch < -90f)
        {
            look = look.WithYaw(look.yaw * -1f);
        }

        var viewAngles = ViewAngles;
        viewAngles += look;
        viewAngles.pitch = viewAngles.pitch.Clamp(-89f, 89f);
        viewAngles.roll = 0f;
        ViewAngles = viewAngles.Normal;
    }

    public override void FrameSimulate(IClient client)
    {
        SimulateRotation();

        foreach(var simulated in Components.GetAll<IFrameSimulatedComponent>())
            simulated.FrameSimulate(client);
    }

    protected virtual void SimulateRotation()
    {
        Rotation = ViewAngles.WithPitch(0f).ToRotation();
    }

    public Trace CreateTrace(float liftFeet = 0f)
    {
        return CreateTrace(0, 0, liftFeet);
    }

    public Trace CreateTrace(Vector3 start, Vector3 end, float liftFeet = 0f)
    {
        return CreateTrace(start, end, Hull.Mins, Hull.Maxs, liftFeet);
    }

    public virtual Trace CreateTrace(Vector3 start, Vector3 end, Vector3 mins, Vector3 maxs, float liftFeet = 0f)
    {
        if(liftFeet > 0)
        {
            start += Vector3.Up * liftFeet;
            maxs = maxs.WithZ(maxs.z - liftFeet);
        }

        return Trace.Ray(start, end)
                    .Size(mins, maxs)
                    .WithAnyTags("solid", "playerclip", "passbullets")
                    .Ignore(this);
    }

    public bool HasAnimatingEvent(string eventName)
    {
        return Controller?.HasEvent(eventName) ?? false;
    }


    public virtual void SetHull(BBox bBox) => Hull = bBox;

    public virtual void SetPhysicsHeight(float height)
    {
        var newHull = new BBox(Hull.Mins, Hull.Maxs.WithZ(height));
        if(HullUpdater is null)
            Hull = newHull;
        else
            HullUpdater.SetTarget(newHull);

        CameraSimulator!.EyePositionHeight = height - 16f;
    }
}
