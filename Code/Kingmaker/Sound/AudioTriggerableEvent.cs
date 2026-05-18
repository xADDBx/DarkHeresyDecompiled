using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.Sound;

public class AudioTriggerableEvent : AkAudioTriggerable
{
	[SerializeField]
	protected AkEventReference m_Event;

	[SerializeField]
	protected bool m_ActionMode;

	[SerializeField]
	[ShowIf("m_ActionMode")]
	protected AkActionOnEventType m_Action;

	[SerializeField]
	[ShowIf("m_ActionMode")]
	protected float m_TransitionDuration;

	[SerializeField]
	[ShowIf("m_ActionMode")]
	protected AkCurveInterpolation m_CurveInterpolation = AkCurveInterpolation.AkCurveInterpolation_Linear;

	public override void OnTrigger()
	{
		if (m_ActionMode)
		{
			m_Event.ExecuteAction(base.gameObject, m_Action, (int)(1000f * m_TransitionDuration), m_CurveInterpolation);
		}
		else
		{
			m_Event.Post(base.gameObject);
		}
	}

	protected override void OnStop(int fade)
	{
		m_Event.ExecuteAction(base.gameObject, AkActionOnEventType.AkActionOnEventType_Stop, fade, AkCurveInterpolation.AkCurveInterpolation_Linear);
	}
}
