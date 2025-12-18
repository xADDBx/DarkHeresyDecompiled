using System;
using System.Collections.Generic;
using Owlcat.Runtime.Visual.Effects.GlobalEffects.Components;
using Owlcat.Runtime.Visual.Effects.GlobalEffects.Controllers;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Effects.GlobalEffects;

[RequireComponent(typeof(Volume))]
public class GlobalEffect : MonoBehaviour
{
	public static Action<Camera, VolumeStack> OnAfterVolumeStackUpdated;

	private List<ControllerBase> m_Controllers = new List<ControllerBase>();

	private GlobalEffectContext m_Context = new GlobalEffectContext();

	private Volume m_Volume;

	[SerializeField]
	private GlobalEffectProfile m_Profile;

	[SerializeField]
	private bool m_PlayOnStart = true;

	[SerializeField]
	private GlobalEffectStateMachine m_State;

	public GlobalEffectProfile Profile
	{
		get
		{
			return m_Profile;
		}
		set
		{
			m_Profile = value;
		}
	}

	public GlobalEffectStateMachine State => m_State;

	public bool PlayOnStart
	{
		get
		{
			return m_PlayOnStart;
		}
		set
		{
			m_PlayOnStart = value;
		}
	}

	private void OnEnable()
	{
		OnAfterVolumeStackUpdated = (Action<Camera, VolumeStack>)Delegate.Combine(OnAfterVolumeStackUpdated, new Action<Camera, VolumeStack>(OnAfterVolumeStackUpdatedHandler));
		Init();
		if (m_PlayOnStart && m_State.WeightMode == WeightMode.Automatic)
		{
			m_State.LifetimeState = GlobalEffectLifetimeState.FadeIn;
		}
	}

	private void OnDisable()
	{
		OnAfterVolumeStackUpdated = (Action<Camera, VolumeStack>)Delegate.Remove(OnAfterVolumeStackUpdated, new Action<Camera, VolumeStack>(OnAfterVolumeStackUpdatedHandler));
		CleanUp();
	}

	private void Init()
	{
		if (m_Profile != null)
		{
			m_Controllers.Clear();
			RefreshContext(null, null);
			foreach (ComponentBase component in m_Profile.Components)
			{
				ControllerBase controllerBase = component.CreateController();
				controllerBase.Initialize(m_Context);
				m_Controllers.Add(controllerBase);
			}
		}
		TryGetComponent<Volume>(out m_Volume);
	}

	private void CleanUp()
	{
		foreach (ControllerBase controller in m_Controllers)
		{
			controller.CleanUp();
		}
		m_Controllers.Clear();
	}

	private void OnAfterVolumeStackUpdatedHandler(Camera camera, VolumeStack stack)
	{
		RefreshContext(camera, stack);
		foreach (ControllerBase controller in m_Controllers)
		{
			if (controller.Active)
			{
				controller.UpdateInternal(m_Context);
			}
		}
	}

	private void RefreshContext(Camera camera, VolumeStack stack)
	{
		m_Context.GlobalEffect = this;
		m_Context.Camera = camera;
		m_Context.VolumeStack = stack;
	}

	private void Update()
	{
		m_State.Tick();
		m_Volume.weight = m_State.Weight;
	}

	internal void Play()
	{
		m_State.LifetimeState = GlobalEffectLifetimeState.FadeIn;
	}

	internal void Stop()
	{
		m_State.LifetimeState = GlobalEffectLifetimeState.Finished;
	}
}
