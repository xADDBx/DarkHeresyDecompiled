using System;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Events;

[Serializable]
public class AnimationClipEventLinkTraversal : AnimationClipEvent
{
	[SerializeField]
	private TraversalAnimationEventType m_Type;

	public TraversalAnimationEventType Type
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

	public AnimationClipEventLinkTraversal(float time)
		: base(time)
	{
		m_Type = TraversalAnimationEventType.StartTraverse;
	}

	public AnimationClipEventLinkTraversal(float time, TraversalAnimationEventType type)
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
		return new AnimationClipEventLinkTraversal(base.Time, Type);
	}

	public override string ToString()
	{
		return $"{Type} event at {base.Time}";
	}
}
