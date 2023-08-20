using EasyWeapons.Demo.Entities.Components;
using EasyWeapons.Demo.Entities.Pawns;
using EasyWeapons.Entities.Components;
using Sandbox;
using System;
using System.ComponentModel;

namespace EasyWepons.Demo.Entities.Components;

public partial class CameraSimulator : EntityComponent<DefaultPawn>, IResetableComponent, ISimulatedComponent
{
    [Net, Local]
    public float DefaultThirdPersonCameraMaxDistance { get; set; } = 80;

    [Net, Local]
    public float DefaultThirdPersonCameraAngle { get; set; } = 16;

    [Net, Local]
    public float DefaultEyePositionHeight { get; set; } = 64;

    [Net, Local]
    public bool DefaultThirdPersonState { get; set; } = false;


    [Net, Predicted, Local]
    public float EyePositionHeight { get; set; } = 64;

    [Net, Predicted, Local]
    public float ThirdPersonCameraMaxDistance { get; set; } = 80f;

    [Net, Predicted, Local]
    public float ThirdPersonCameraAngle { get; set; } = 16f;

    [Net, Predicted, Local]
    public bool IsThirdPerson { get; set; } = false;

    [Net, Predicted, Local]
    public bool IsThirdPersonInverted { get; set; } = false;

    [Net, Predicted, Local]
    public Transform CameraTransform { get; set; }


    [Browsable(false)]
    public Vector3 EyePosition
    {
        get => Entity.Transform.PointToWorld(EyeLocalPosition);
        set => EyeLocalPosition = Entity.Transform.PointToLocal(value);
    }

    [Net, Predicted, Browsable(false)]
    public Vector3 EyeLocalPosition { get; set; }

    [Browsable(false)]
    public Rotation EyeRotation
    {
        get => Entity.Transform.RotationToWorld(EyeLocalRotation);
        set => EyeLocalRotation = Entity.Transform.RotationToLocal(value);
    }

    [Net, Predicted, Browsable(false)]
    public Rotation EyeLocalRotation { get; set; }

    public virtual Ray AimRay => new(CameraTransform.Position, CameraTransform.Rotation.Forward);

    public virtual void Simulate(IClient client)
    {
        SimulateRotation();
        EyeLocalPosition = Vector3.Up * EyePositionHeight;
        CameraTransform = CalculateCameraTransform();
    }

    [GameEvent.Client.PostCamera]
    protected virtual void OnPostCamera()
    {
        if(Entity.IsAuthority == false)
            return;
        SimulateRotation();

        var cameraTransform = CalculateCameraTransform(IsThirdPersonInverted);

        Camera.Rotation = cameraTransform.Rotation;
        Camera.Position = cameraTransform.Position;

        Camera.FieldOfView = Screen.CreateVerticalFieldOfView(Game.Preferences.FieldOfView);
        Camera.FirstPersonViewer = IsThirdPerson ? null : Entity;
    }

    public virtual void ChangeView()
    {
        if(IsThirdPerson)
        {
            if(IsThirdPersonInverted)
                IsThirdPersonInverted = IsThirdPerson = false;
            else
                IsThirdPersonInverted = true;
        }
        else
        {
            IsThirdPerson = true;
            IsThirdPersonInverted = false;
        }
    }

    public virtual void Reset()
    {
        ThirdPersonCameraMaxDistance = DefaultThirdPersonCameraMaxDistance;
        ThirdPersonCameraAngle = DefaultThirdPersonCameraAngle;
        EyePositionHeight = DefaultEyePositionHeight;
        IsThirdPerson = DefaultThirdPersonState;
        CameraTransform = new();
    }

    protected virtual void SimulateRotation()
    {
        EyeRotation = Entity.ViewAngles.ToRotation();
    }

    protected virtual Transform CalculateCameraTransform(bool inverted = false)
    {
        var result = new Transform
        {
            Rotation = Entity.ViewAngles.ToRotation(),
            Position = EyePosition
        };

        if(IsThirdPerson)
            result = GetThirdPersonCameraTransform(result, ThirdPersonCameraMaxDistance, ThirdPersonCameraAngle, inverted);

        return result;
    }

    protected virtual Transform GetThirdPersonCameraTransform(Transform firstPersonTransform, float distance, float angle, bool inverted = false)
    {
        var rotation = firstPersonTransform.Rotation;
        if(inverted)
            rotation = Rotation.LookAt(rotation.Backward, rotation.Up);
        var offsetRotation = Rotation.FromAxis(Vector3.Up, angle) * rotation;

        distance *= Entity.LocalScale;
        Vector3 targetPos = firstPersonTransform.Position + offsetRotation.Right;
        targetPos += offsetRotation.Forward * -distance;

        firstPersonTransform.Position = ClampThirdPersonCamera(firstPersonTransform.Position, targetPos);
        firstPersonTransform.Rotation = rotation;

        return firstPersonTransform;
    }

    protected virtual Vector3 ClampThirdPersonCamera(Vector3 center, Vector3 targetPosition)
    {
        var maxRadius = GetMaxTraceRadius(8f, center, targetPosition);

        var tr = Trace.Ray(center, targetPosition)
                .WithAnyTags("solid")
                .Ignore(Entity)
                .Radius(maxRadius)
                .Run();

        return tr.EndPosition;
    }

    protected virtual float GetMaxTraceRadius(float maxRadius, Vector3 center, Vector3 targetPosition)
    {
        var hullGlobal = Entity.Hull + Entity.Position;

        bool centerInsideHull = hullGlobal.Contains(center);
        if(centerInsideHull)
        {
            var maxsDiff = (hullGlobal.Maxs - center).Abs();
            var minsDiff = (hullGlobal.Mins - center).Abs();

            var minComponents = maxsDiff.ComponentMin(minsDiff);

            maxRadius = MathF.Min(maxRadius, minComponents.x);
            maxRadius = MathF.Min(maxRadius, minComponents.y);
            maxRadius = MathF.Min(maxRadius, minComponents.z);
        }

        maxRadius = MathF.Max(maxRadius, 0);

        return maxRadius;
    }

}
