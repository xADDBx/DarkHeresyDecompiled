using Kingmaker.View;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker;

public class MovementSettingsFromAnimations : IMovementSettingsProvider
{
	private readonly UnitAnimationManager m_AnimationManager;

	private AnimationSet m_AnimationSet;

	private float m_Acceleration = 10f;

	private bool m_DecelerateBeforeStop = true;

	private float m_StoppingDistance = 1.35f;

	private float m_MinSpeed = 0.2f;

	private float m_AngularSpeedInCombat = 360f;

	private float m_AngularSpeedInNonCombat = 180f;

	private float m_AngularSpeedWhenMove = 220f;

	private float m_SlowDownCoefficient = 0.7f;

	public float Acceleration
	{
		get
		{
			UpdateSettings();
			return m_Acceleration;
		}
	}

	public bool DecelerateBeforeStop
	{
		get
		{
			UpdateSettings();
			return m_DecelerateBeforeStop;
		}
	}

	public float StoppingDistance
	{
		get
		{
			UpdateSettings();
			return m_StoppingDistance;
		}
	}

	public float MinSpeed
	{
		get
		{
			UpdateSettings();
			return m_MinSpeed;
		}
	}

	public float AngularSpeedInCombat
	{
		get
		{
			UpdateSettings();
			return m_AngularSpeedInCombat;
		}
	}

	public float AngularSpeedInNonCombat
	{
		get
		{
			UpdateSettings();
			return m_AngularSpeedInNonCombat;
		}
	}

	public float AngularSpeedWhenMove
	{
		get
		{
			UpdateSettings();
			return m_AngularSpeedWhenMove;
		}
	}

	public float SlowDownCoefficient
	{
		get
		{
			UpdateSettings();
			return m_SlowDownCoefficient;
		}
	}

	public MovementSettingsFromAnimations(UnitAnimationManager unitAnimationManager)
	{
		m_AnimationManager = unitAnimationManager;
	}

	private void UpdateSettings()
	{
		if (!(m_AnimationManager == null) && !(m_AnimationManager.AnimationSet == null) && (Application.isEditor || !(m_AnimationSet == m_AnimationManager.AnimationSet)))
		{
			m_AnimationSet = m_AnimationManager.AnimationSet;
			if (m_AnimationSet.GetAction(UnitAnimationType.LocoMotion) is UnitAnimationActionLocomotion unitAnimationActionLocomotion)
			{
				m_Acceleration = unitAnimationActionLocomotion.Acceleration;
				m_DecelerateBeforeStop = !unitAnimationActionLocomotion.UseCustomStoppingAnimation;
				m_StoppingDistance = unitAnimationActionLocomotion.StoppingDistance;
				m_MinSpeed = unitAnimationActionLocomotion.MinSpeed;
				m_AngularSpeedInCombat = unitAnimationActionLocomotion.AngularSpeedInCombat;
				m_AngularSpeedInNonCombat = unitAnimationActionLocomotion.AngularSpeedInNonCombat;
				m_AngularSpeedWhenMove = unitAnimationActionLocomotion.AngularSpeedWhenMove;
				m_SlowDownCoefficient = unitAnimationActionLocomotion.SlowDownCoefficient;
			}
		}
	}
}
