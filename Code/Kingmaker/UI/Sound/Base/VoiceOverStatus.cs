using System;
using Kingmaker.Sound.Base;

namespace Kingmaker.UI.Sound.Base;

[Serializable]
public class VoiceOverStatus
{
	private const double DelayTimeSec = 0.25;

	private TimeSpan m_EndTime;

	private TimeSpan? m_PauseTime;

	private bool m_PlayTriggered;

	private bool m_Stopped;

	public uint PlayingSoundId { get; set; }

	public bool IsEnded
	{
		get
		{
			if (!m_PlayTriggered || m_PauseTime.HasValue || !(Game.Instance.Player.RealTime > m_EndTime))
			{
				return m_Stopped;
			}
			return true;
		}
	}

	public bool Stopped => m_Stopped;

	public float RemainingTime
	{
		get
		{
			if (!m_PauseTime.HasValue)
			{
				return (float)(m_EndTime - Game.Instance.Player.RealTime).TotalSeconds;
			}
			return float.PositiveInfinity;
		}
	}

	public event Action Ended;

	public VoiceOverStatus(TimeSpan startTime)
	{
		m_EndTime = startTime + TimeSpan.FromSeconds(0.25);
		m_PlayTriggered = false;
	}

	public void HandleCallback(object cookie, AkCallbackType type, object info)
	{
		switch (type)
		{
		case AkCallbackType.AK_EndOfEvent:
			m_PlayTriggered = true;
			break;
		case AkCallbackType.AK_AudioInterruption:
			m_Stopped = true;
			break;
		case AkCallbackType.AK_Duration:
			if (!(info is AkDurationCallbackInfo akDurationCallbackInfo))
			{
				PFLog.VO.Warning("[VO] AK_Duration callback received unexpected info type");
				break;
			}
			m_EndTime += TimeSpan.FromMilliseconds(akDurationCallbackInfo.fDuration);
			m_PlayTriggered = true;
			break;
		}
	}

	public void Stop()
	{
		if (!m_Stopped)
		{
			SoundEventsManager.StopPlayingById(PlayingSoundId);
			m_Stopped = true;
			this.Ended?.Invoke();
		}
	}

	public void Pause()
	{
		if (!IsEnded)
		{
			m_PauseTime = Game.Instance.Player.RealTime;
		}
	}

	public void Resume()
	{
		if (m_PauseTime.HasValue)
		{
			m_EndTime += Game.Instance.Player.RealTime - m_PauseTime.Value;
			m_PauseTime = null;
		}
	}
}
