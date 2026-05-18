using Kingmaker.Controllers;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Sound.Base;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class SubtitleBarkHandle : BarkHandleBase
{
	public SubtitleBarkHandle(string text, float duration, VoiceOverStatus voiceOverStatus)
	{
		if (Game.Instance.Controllers.CustomUpdateController.TryFind((IUpdatable x) => x is SubtitleBarkHandle, out var result))
		{
			((IBarkHandle)result).InterruptBark();
		}
		Init(duration, voiceOverStatus);
		Game.Instance.Controllers.BarkController.TrackHandle(this);
		EventBus.RaiseEvent(delegate(ISubtitleBarkHandler h)
		{
			h.HandleOnShowBark(text);
		});
	}

	protected override bool IsPlayingBarkCore()
	{
		if (m_VoiceOverStatus == null)
		{
			return m_RemainingTime > 0f;
		}
		return Mathf.Max(m_VoiceOverStatus.RemainingTime, m_RemainingTime) > 0f;
	}

	public override void InterruptBark()
	{
		if (m_IsActive)
		{
			base.InterruptBark();
			EventBus.RaiseEvent(delegate(ISubtitleBarkHandler h)
			{
				h.HandleOnHideBark();
			});
		}
	}
}
