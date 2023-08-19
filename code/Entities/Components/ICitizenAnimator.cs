using Sandbox;

namespace EasyWepons.Demo.Entities.Components;

public interface ICitizenAnimator : IComponent
{
    void Animate(CitizenAnimationHelper helper);
}
