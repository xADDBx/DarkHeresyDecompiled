using System;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Controllers;
using Kingmaker.UI.Sound.Base;

namespace Kingmaker.Code.UI.MVVM;

public abstract class BarkHandleBase : IBarkHandle, IUpdatable
{
	protected VoiceOverStatus m_VoiceOverStatus;

	protected float m_RemainingTime;

	protected bool m_IsActive = true;

	public VoiceOverStatus VoiceOverStatus => m_VoiceOverStatus;

	private event Action BarkEndCallbackActions;

	protected void Init(float duration, VoiceOverStatus voiceOverStatus)
	{
		m_VoiceOverStatus = voiceOverStatus;
		m_RemainingTime = ((duration > 0f) ? duration : UtilityBark.DefaultBarkTime);
		if (m_VoiceOverStatus != null)
		{
			m_VoiceOverStatus.Ended += OnBarkEnded;
		}
		Game.Instance.Controllers.CustomUpdateController.Add(this);
	}

	protected void OnBarkEnded()
	{
		this.BarkEndCallbackActions?.Invoke();
	}

	public void AddCallback(Action callback)
	{
		BarkEndCallbackActions += callback;
	}

	public bool IsPlayingBark()
	{
		if (!m_IsActive)
		{
			return false;
		}
		return IsPlayingBarkCore();
	}

	protected abstract bool IsPlayingBarkCore();

	public virtual void InterruptBark()
	{
		if (m_IsActive)
		{
			m_IsActive = false;
			Game.Instance.Controllers.BarkController.UntrackHandle(this);
			Game.Instance.Controllers.CustomUpdateController.Remove(this);
			if (m_VoiceOverStatus != null)
			{
				m_VoiceOverStatus.Stop();
				m_VoiceOverStatus = null;
			}
			else
			{
				OnBarkEnded();
			}
			m_RemainingTime = -1f;
		}
	}

	void IUpdatable.Tick(float delta)
	{
		OnTick(delta);
	}

	protected virtual void OnTick(float delta)
	{
		if (!IsPlayingBark())
		{
			InterruptBark();
		}
		else
		{
			m_RemainingTime -= Game.Instance.Controllers.TimeController.DeltaTime;
		}
	}
}
