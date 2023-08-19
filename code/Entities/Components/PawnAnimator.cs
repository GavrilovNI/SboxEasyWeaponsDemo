using Sandbox;
using System.Linq;
using EasyWeapons.Demo.Entities.Components;
using EasyWeapons.Entities.Components;

namespace EasyWepons.Demo.Entities.Components;

public class PawnAnimator : SpecificEntityComponent<AnimatedEntity, IAnimatableCitizen>, ISingletonComponent, ICitizenAnimator, ISimulatedComponent
{
    protected virtual void AnimateDefault(CitizenAnimationHelper helper)
    {
        helper.WithVelocity(Entity.Velocity);
        helper.HoldType = CitizenAnimationHelper.HoldTypes.None;
        helper.IsGrounded = Entity.GroundEntity.IsValid();

        helper.WithLookAt(SpecificEntity.EyeLookingRay.Position + SpecificEntity.EyeLookingRay.Forward * 100);

        if(SpecificEntity.HasAnimatingEvent("jump"))
            helper.TriggerJump();
    }

    protected virtual void AnimateByComponents(CitizenAnimationHelper helper)
    {
        var animators = Entity.Components.GetAll<ICitizenAnimator>().Except(new ICitizenAnimator[] { this });
        foreach(var animator in animators)
            animator.Animate(helper);
    }

    public virtual void Animate(CitizenAnimationHelper helper)
    {
        AnimateDefault(helper);
        AnimateByComponents(helper);

        if(Entity is ICitizenAnimator owner)
            owner.Animate(helper);
    }

    public virtual void Simulate(IClient client)
    {
        if(object.ReferenceEquals(Entity.Client, client) == false)
            return;

        Animate(new CitizenAnimationHelper(Entity));
    }
}
