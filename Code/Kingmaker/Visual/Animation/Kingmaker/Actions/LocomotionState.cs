namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

public abstract class LocomotionState : UnitAnimationActionState<LocomotionStateType, UnitAnimationActionLocomotion>
{
	protected UnitAnimationActionLocomotion.CommonLocomotionData m_RuntimeData => (m_Handle.ActionData as UnitAnimationActionLocomotion.ActionData)?.Data;

	protected float m_MaxSpeed => m_AnimationAction.SprintParameters.Speed;

	protected WeaponAnimationStyle m_ActualWeaponStyle
	{
		get
		{
			if (!m_Handle.Manager.IsInCombat)
			{
				return WeaponAnimationStyle.NonCombat;
			}
			return m_Handle.WeaponStyle;
		}
	}

	protected LocomotionState(UnitAnimationActionLocomotion animationAction, UnitAnimationActionHandle handle)
		: base(animationAction, handle)
	{
	}
}
