using UnityEngine;

namespace Kingmaker.Visual.Animation.Events;

public class AnimationClipEventAiming : AnimationClipEvent
{
	[SerializeField]
	private AimingAnimationEventType m_Type;

	public AimingAnimationEventType Type
	{
		get
		{
			return m_Type;
		}
		set
		{
			m_Type = value;
		}
	}

	public AnimationClipEventAiming(float time)
		: base(time)
	{
		m_Type = AimingAnimationEventType.Start;
	}

	public AnimationClipEventAiming(float time, AimingAnimationEventType type)
		: base(time)
	{
		m_Type = type;
	}

	public override void Start(IAnimationManager animationManager)
	{
	}

	public override object Clone()
	{
		return new AnimationClipEventAiming(base.Time, Type);
	}

	public override string ToString()
	{
		return $"{Type} event at {base.Time}";
	}
}
