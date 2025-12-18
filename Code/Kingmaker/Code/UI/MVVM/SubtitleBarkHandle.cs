using System;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Controllers;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Sound.Base;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class SubtitleBarkHandle : IBarkHandle, IUpdatable
{
	private VoiceOverStatus m_VoiceOverStatus;

	private float m_RemainingTime;

	public VoiceOverStatus VoiceOverStatus => m_VoiceOverStatus;

	private event Action BarkEndCallbackActions;

	public SubtitleBarkHandle(string text, float duration, VoiceOverStatus voiceOverStatus)
	{
		m_VoiceOverStatus = voiceOverStatus;
		m_RemainingTime = ((duration > 0f) ? duration : UtilityBark.DefaultBarkTime);
		if (m_VoiceOverStatus != null)
		{
			m_VoiceOverStatus.Ended += OnBarkEnded;
		}
		Game.Instance.Controllers.CustomUpdateController.Add(this);
		EventBus.RaiseEvent(delegate(ISubtitleBarkHandler h)
		{
			h.HandleOnShowBark(text);
		});
	}

	void IUpdatable.Tick(float delta)
	{
		if (m_RemainingTime > 0f)
		{
			m_RemainingTime -= Game.Instance.Controllers.TimeController.DeltaTime;
		}
		if (!IsPlayingBark())
		{
			InterruptBark();
		}
	}

	public bool IsPlayingBark()
	{
		if (m_VoiceOverStatus == null)
		{
			return m_RemainingTime > 0f;
		}
		return Mathf.Max(m_VoiceOverStatus.RemainingTime, m_RemainingTime) > 0f;
	}

	public void InterruptBark()
	{
		m_RemainingTime = 0f;
		if (m_VoiceOverStatus != null)
		{
			m_VoiceOverStatus?.Stop();
			OnBarkEnded();
		}
		m_VoiceOverStatus = null;
		Game.Instance.Controllers.CustomUpdateController.Remove(this);
		EventBus.RaiseEvent(delegate(ISubtitleBarkHandler h)
		{
			h.HandleOnHideBark();
		});
	}

	private void OnBarkEnded()
	{
		this.BarkEndCallbackActions?.Invoke();
	}

	public void AddCallback(Action callback)
	{
		BarkEndCallbackActions += callback;
	}
}
