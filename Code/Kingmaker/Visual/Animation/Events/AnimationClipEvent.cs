using System;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Events;

[Serializable]
public class AnimationClipEvent : ICloneable, IComparable
{
	[SerializeField]
	private float m_Time;

	[SerializeField]
	private bool m_IsLooped;

	public float Time
	{
		get
		{
			return m_Time;
		}
		set
		{
			if (value < 0f)
			{
				value = 0f;
			}
			m_Time = value;
		}
	}

	public bool IsLooped
	{
		get
		{
			return m_IsLooped;
		}
		set
		{
			m_IsLooped = value;
		}
	}

	public object UserData { get; set; }

	public AnimationClipEvent()
	{
	}

	public AnimationClipEvent(float time)
		: this(time, isLooped: false)
	{
	}

	public AnimationClipEvent(float time, bool isLooped)
	{
		m_Time = time;
		m_IsLooped = isLooped;
	}

	public virtual void Start(IAnimationManager animationManager)
	{
		throw new NotImplementedException();
	}

	public virtual object Clone()
	{
		return new AnimationClipEvent(Time);
	}

	public int CompareTo(object @object)
	{
		if (@object == null)
		{
			throw new ArgumentNullException("object");
		}
		if (!(@object is AnimationClipEvent animationClipEvent))
		{
			throw new InvalidCastException($"{@object} is of type {@object.GetType()} and is not of type {typeof(AnimationClipEvent)}.");
		}
		return Mathf.RoundToInt((Time - animationClipEvent.Time) * 1000f);
	}

	public override string ToString()
	{
		return string.Format("{0}AnimationClipEvent at {1}", IsLooped ? "Looped " : "", Time);
	}
}
