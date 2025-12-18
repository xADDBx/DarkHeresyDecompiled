using System;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Events;

public class AnimationClipEventHitReact : AnimationClipEvent
{
	[SerializeField]
	private HitReactAnimationEventType m_Type;

	public HitReactAnimationEventType Type
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

	public AnimationClipEventHitReact(float time)
		: base(time)
	{
		m_Type = HitReactAnimationEventType.Hit;
	}

	public AnimationClipEventHitReact(float time, HitReactAnimationEventType type)
		: base(time)
	{
		m_Type = type;
	}

	public override Action Start(IAnimationManager animationManager)
	{
		return null;
	}

	public override object Clone()
	{
		return new AnimationClipEventHitReact(base.Time, Type);
	}

	public override string ToString()
	{
		return $"{Type} event at {base.Time}";
	}
}
