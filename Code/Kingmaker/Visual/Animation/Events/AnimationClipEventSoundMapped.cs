using System;
using Kingmaker.Enums.Sound;
using Kingmaker.View.Mechanics;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Events;

[Serializable]
public class AnimationClipEventSoundMapped : AnimationClipEventSound
{
	[SerializeField]
	private MappedAnimationEventType m_Type;

	public MappedAnimationEventType Type
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

	public AnimationClipEventSoundMapped()
	{
	}

	public AnimationClipEventSoundMapped(float time)
		: this(time, MappedAnimationEventType.Idle)
	{
	}

	public AnimationClipEventSoundMapped(float time, MappedAnimationEventType type)
		: base(time)
	{
		m_Type = type;
	}

	public override Action PostSoundEvent(MechanicEntityView view)
	{
		view.PostSoundEventMapped(Type);
		return null;
	}

	public override object Clone()
	{
		return new AnimationClipEventSoundMapped(base.Time, Type);
	}

	public override string ToString()
	{
		return $"{Type} mapped sound event at {base.Time}";
	}
}
