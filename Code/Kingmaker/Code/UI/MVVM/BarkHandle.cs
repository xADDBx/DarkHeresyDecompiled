using System;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Signals;
using Kingmaker.UI.Sound.Base;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BarkHandle : IBarkHandle, IUpdatable
{
	private readonly Entity m_Entity;

	private VoiceOverStatus m_VoiceOverStatus;

	private float m_RemainingTime;

	private bool m_IsActive = true;

	private SignalWrapper m_StopPlaySignal;

	public VoiceOverStatus VoiceOverStatus => m_VoiceOverStatus;

	public event Action BarkEndCallbackActions;

	private BarkHandle(Entity entity, float duration = -1f, VoiceOverStatus voiceOverStatus = null, bool synced = true)
	{
		if (Game.Instance.Controllers.CustomUpdateController.TryFind((IUpdatable x) => x is BarkHandle barkHandle && barkHandle.m_Entity == entity, out var result))
		{
			((IBarkHandle)result).InterruptBark();
		}
		m_Entity = entity;
		m_VoiceOverStatus = voiceOverStatus;
		m_RemainingTime = ((duration > 0f) ? duration : UtilityBark.DefaultBarkTime);
		m_StopPlaySignal = (synced ? SignalService.Instance.RegisterNext() : SignalWrapper.Empty);
		if (m_VoiceOverStatus != null)
		{
			m_VoiceOverStatus.Ended += OnBarkEnded;
		}
		Game.Instance.Controllers.CustomUpdateController.Add(this);
	}

	private void OnBarkEnded()
	{
		this.BarkEndCallbackActions?.Invoke();
	}

	public BarkHandle(Entity entity, string text, float duration = -1f, VoiceOverStatus voiceOverStatus = null, bool synced = true)
		: this(entity, duration, voiceOverStatus, synced)
	{
		EventBus.RaiseEvent((IEntity)entity, (Action<IBarkHandler>)delegate(IBarkHandler h)
		{
			h.HandleOnShowBark(text);
		}, isCheckRuntime: true);
	}

	public BarkHandle(Entity entity, string text, string encyclopediaLink, float duration = -1f, VoiceOverStatus voiceOverStatus = null)
		: this(entity, duration, voiceOverStatus)
	{
		EventBus.RaiseEvent((IEntity)entity, (Action<IBarkHandler>)delegate(IBarkHandler h)
		{
			h.HandleOnShowLinkedBark(text, encyclopediaLink);
		}, isCheckRuntime: true);
	}

	void IUpdatable.Tick(float delta)
	{
		if (!IsPlayingBark())
		{
			InterruptBark();
			return;
		}
		m_RemainingTime -= Game.Instance.Controllers.TimeController.DeltaTime;
		if (!m_Entity.IsInGame)
		{
			InterruptBark();
		}
	}

	public bool IsPlayingBark()
	{
		if (!m_IsActive)
		{
			return false;
		}
		if ((m_VoiceOverStatus == null) ? (m_RemainingTime > 0f) : (Mathf.Max(m_VoiceOverStatus.RemainingTime, m_RemainingTime) > 0f))
		{
			return true;
		}
		return !SignalService.Instance.CheckReadyOrSend(ref m_StopPlaySignal, emptyIsOk: true);
	}

	public void InterruptBark()
	{
		if (m_IsActive)
		{
			m_IsActive = false;
			EventBus.RaiseEvent((IEntity)m_Entity, (Action<IBarkHandler>)delegate(IBarkHandler h)
			{
				h.HandleOnHideBark();
			}, isCheckRuntime: true);
			Game.Instance.Controllers.CustomUpdateController.Remove(this);
			if (m_VoiceOverStatus != null)
			{
				m_VoiceOverStatus.Stop();
			}
			else
			{
				OnBarkEnded();
			}
			m_VoiceOverStatus = null;
			m_RemainingTime = -1f;
			m_StopPlaySignal = SignalWrapper.Empty;
		}
	}

	public void AddCallback(Action callback)
	{
		BarkEndCallbackActions += callback;
	}
}
