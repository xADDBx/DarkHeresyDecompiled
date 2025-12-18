using Animancer.FSM;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

public abstract class UnitAnimationActionState<TStateType, TAnimationAction> : State where TAnimationAction : UnitAnimationAction
{
	protected readonly TAnimationAction m_AnimationAction;

	protected readonly UnitAnimationActionHandle m_Handle;

	protected UnitAnimationActionState(TAnimationAction animationAction, UnitAnimationActionHandle handle)
	{
		m_AnimationAction = animationAction;
		m_Handle = handle;
	}

	public abstract TStateType Tick(float deltaTime);
}
