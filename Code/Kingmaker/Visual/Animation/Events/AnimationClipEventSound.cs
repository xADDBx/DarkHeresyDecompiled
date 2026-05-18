using System;
using Animancer;
using Kingmaker.View.Mechanics;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Events;

[Serializable]
public class AnimationClipEventSound : AnimationClipEvent
{
	[SerializeField]
	private string m_Name;

	[SerializeField]
	private string m_StopName;

	[SerializeField]
	private float m_Volume;

	public string Name
	{
		get
		{
			return m_Name;
		}
		set
		{
			m_Name = value;
		}
	}

	public string StopName
	{
		get
		{
			return m_StopName;
		}
		set
		{
			m_StopName = value;
		}
	}

	public float Volume => m_Volume;

	public AnimancerState AnimancerState => (AnimancerState)base.UserData;

	public AnimationClipEventSound()
	{
	}

	public AnimationClipEventSound(float time)
		: this(time, isLooped: false, null, null, 1f)
	{
	}

	public AnimationClipEventSound(float time, bool isLooped, string name, string stopName, float volume)
		: base(time, isLooped)
	{
		m_Name = name;
		m_StopName = stopName;
		m_Volume = volume;
	}

	public sealed override void Start(IAnimationManager animationManager)
	{
		animationManager.SoundEventsManager.PostSoundEvent(this);
	}

	public virtual Action PostSoundEvent(MechanicEntityView view)
	{
		uint uniqueId = view.PostSoundEvent(Name, Volume);
		if (!base.IsLooped)
		{
			return null;
		}
		if (!string.IsNullOrEmpty(StopName))
		{
			return delegate
			{
				view.PostSoundEvent(StopName, Volume);
			};
		}
		return delegate
		{
			view.StopPlayingSoundById(uniqueId);
		};
	}

	public override object Clone()
	{
		return new AnimationClipEventSound(base.Time, base.IsLooped, Name, StopName, Volume);
	}

	public override string ToString()
	{
		return $"{Name} sound event at {base.Time}";
	}
}
