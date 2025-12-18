using System;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Effects.GlobalEffects;

[Serializable]
public class GlobalEffectStateMachine
{
	[SerializeField]
	private WeightMode m_WeightMode;

	[SerializeField]
	[Range(0f, 1f)]
	private float m_Weight;

	private float m_StateAge;

	private float m_Age;

	private GlobalEffectLifetimeState m_LifetimeState;

	[SerializeField]
	private GlobalEffectLifetime m_Lifetime = new GlobalEffectLifetime();

	public WeightMode WeightMode
	{
		get
		{
			return m_WeightMode;
		}
		set
		{
			m_WeightMode = value;
		}
	}

	public GlobalEffectLifetimeState LifetimeState
	{
		get
		{
			return m_LifetimeState;
		}
		set
		{
			if (m_LifetimeState != value)
			{
				m_LifetimeState = value;
				m_StateAge = 0f;
				switch (m_LifetimeState)
				{
				case GlobalEffectLifetimeState.NotStarted:
					m_Age = 0f;
					m_Weight = 0f;
					break;
				case GlobalEffectLifetimeState.FadeIn:
					m_Age = 0f;
					m_Weight = 0f;
					break;
				case GlobalEffectLifetimeState.Main:
					m_Age = m_Lifetime.FadeIn;
					m_Weight = 1f;
					break;
				case GlobalEffectLifetimeState.FadeOut:
					m_Age = m_Lifetime.FadeIn + m_Lifetime.Main;
					m_Weight = 1f;
					break;
				case GlobalEffectLifetimeState.Finished:
					m_Age = m_Lifetime.FadeIn + m_Lifetime.Main + m_Lifetime.FadeOut;
					m_Weight = 0f;
					break;
				}
			}
		}
	}

	public GlobalEffectLifetime Lifetime
	{
		get
		{
			return m_Lifetime;
		}
		set
		{
			m_Lifetime = value;
		}
	}

	public float Weight
	{
		get
		{
			return m_Weight;
		}
		set
		{
			if (m_WeightMode == WeightMode.Manual)
			{
				m_Weight = math.clamp(value, 0f, 1f);
			}
			else if (m_WeightMode == WeightMode.Automatic)
			{
				throw new InvalidOperationException("Weight cannot be set in Automatic mode.");
			}
		}
	}

	public float Age => m_Age;

	public void Tick()
	{
		if (m_LifetimeState != 0 && m_LifetimeState != GlobalEffectLifetimeState.Finished)
		{
			m_StateAge += Time.deltaTime;
			m_Age += Time.deltaTime;
		}
		if (m_WeightMode == WeightMode.Automatic)
		{
			TickAutomatickState();
		}
	}

	private void TickAutomatickState()
	{
		switch (m_LifetimeState)
		{
		case GlobalEffectLifetimeState.NotStarted:
			break;
		case GlobalEffectLifetimeState.FadeIn:
			if (m_Lifetime.FadeIn > 0f)
			{
				if (m_StateAge >= m_Lifetime.FadeIn)
				{
					m_LifetimeState = GlobalEffectLifetimeState.Main;
					m_StateAge = 0f;
					m_Weight = 1f;
				}
				else
				{
					m_Weight = math.clamp(m_StateAge / m_Lifetime.FadeIn, 0f, 1f);
				}
			}
			else
			{
				m_LifetimeState = GlobalEffectLifetimeState.Main;
				m_StateAge = 0f;
			}
			break;
		case GlobalEffectLifetimeState.Main:
			m_Weight = 1f;
			if (m_Lifetime.Main > 0f && m_StateAge >= m_Lifetime.Main)
			{
				m_LifetimeState = GlobalEffectLifetimeState.FadeOut;
				m_StateAge = 0f;
			}
			break;
		case GlobalEffectLifetimeState.FadeOut:
			if (m_Lifetime.FadeOut > 0f)
			{
				if (m_StateAge >= m_Lifetime.FadeOut)
				{
					m_Weight = 0f;
					m_LifetimeState = GlobalEffectLifetimeState.Finished;
					m_StateAge = 0f;
				}
				else
				{
					m_Weight = math.clamp(1f - m_StateAge / m_Lifetime.FadeOut, 0f, 1f);
				}
			}
			else
			{
				m_LifetimeState = GlobalEffectLifetimeState.Finished;
				m_StateAge = 0f;
			}
			break;
		case GlobalEffectLifetimeState.Finished:
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}
}
