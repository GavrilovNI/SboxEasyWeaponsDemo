using Sandbox;

namespace EasyWeapons.Demo.Entities.Components;

public interface IAnimatableCitizen : IEntity
{
    Ray EyeLookingRay { get; }

    bool HasAnimatingEvent(string eventName);
}
