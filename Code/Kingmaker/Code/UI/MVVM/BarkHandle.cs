using System;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Signals;
using Kingmaker.UI.Sound.Base;

namespace Kingmaker.Code.UI.MVVM;

public class BarkHandle : BarkHandleBase
{
	private readonly Entity m_Entity;

	private SignalWrapper m_StopPlaySignal;

	private BarkHandle(Entity entity, float duration = -1f, VoiceOverStatus voiceOverStatus = null, bool synced = true)
	{
		if (entity == null)
		{
			m_IsActive = false;
			return;
		}
		if (Game.Instance.Controllers.CustomUpdateController.TryFind((IUpdatable x) => x is BarkHandle barkHandle && barkHandle.m_Entity == entity, out var result))
		{
			((IBarkHandle)result).InterruptBark();
		}
		m_Entity = entity;
		m_StopPlaySignal = (synced ? SignalService.Instance.RegisterNext() : SignalWrapper.Empty);
		Init(duration, voiceOverStatus);
		Game.Instance.Controllers.BarkController.Add(m_Entity, this);
		Game.Instance.Controllers.BarkController.TrackHandle(this);
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

	protected override void OnTick(float delta)
	{
		base.OnTick(delta);
		if (!m_Entity.IsInGame)
		{
			InterruptBark();
		}
	}

	protected override bool IsPlayingBarkCore()
	{
		if ((m_VoiceOverStatus == null) ? (m_RemainingTime > 0f) : (!m_VoiceOverStatus.IsEnded))
		{
			return true;
		}
		return !SignalService.Instance.CheckReadyOrSend(ref m_StopPlaySignal, emptyIsOk: true);
	}

	public override void InterruptBark()
	{
		base.InterruptBark();
		EventBus.RaiseEvent((IEntity)m_Entity, (Action<IBarkHandler>)delegate(IBarkHandler h)
		{
			h.HandleOnHideBark();
		}, isCheckRuntime: true);
		m_StopPlaySignal = SignalWrapper.Empty;
	}
}
