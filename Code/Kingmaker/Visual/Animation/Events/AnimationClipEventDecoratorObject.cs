using System;
using Kingmaker.View.Animation;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Events;

public class AnimationClipEventDecoratorObject : AnimationClipEvent
{
	[SerializeField]
	private float m_EndTime;

	[SerializeField]
	private UnitAnimationDecoratorObject m_DecoratorObject;

	public float EndTime
	{
		get
		{
			return m_EndTime;
		}
		set
		{
			if (value < base.Time)
			{
				value = base.Time;
			}
			m_EndTime = value;
		}
	}

	public UnitAnimationDecoratorObject DecoratorObject
	{
		get
		{
			return m_DecoratorObject;
		}
		set
		{
			m_DecoratorObject = value;
		}
	}

	public AnimationClipEventDecoratorObject()
	{
	}

	public AnimationClipEventDecoratorObject(float startTime)
		: base(startTime)
	{
		m_DecoratorObject = null;
		m_EndTime = startTime;
	}

	public AnimationClipEventDecoratorObject(float startTime, float endTime, UnitAnimationDecoratorObject decoratorObject = null)
		: base(startTime)
	{
		m_DecoratorObject = decoratorObject;
		m_EndTime = ((endTime > startTime) ? endTime : startTime);
	}

	public override Action Start(IAnimationManager animationManager)
	{
		animationManager.CallbackReceiver.PostDecoratorObject(DecoratorObject, EndTime - base.Time);
		return null;
	}

	public override object Clone()
	{
		return new AnimationClipEventDecoratorObject(base.Time, EndTime, DecoratorObject);
	}

	public override string ToString()
	{
		return $"{DecoratorObject.name} decorator object event at {base.Time} until {EndTime}";
	}
}
