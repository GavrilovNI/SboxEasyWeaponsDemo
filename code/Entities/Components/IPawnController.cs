using Sandbox;

namespace EasyWeapons.Demo.Entities.Components;

public interface IPawnController : IComponent
{
    bool HasEvent(string eventName);
}
