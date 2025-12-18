using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.Sound;

public class AudioTriggerableState : AkAudioTriggerable
{
	[SerializeField]
	private AkStateReference m_State;

	public bool DelayEnterExit;

	[ShowIf("DelayEnterExit")]
	public float m_DelaySeconds = 5f;

	private AudioZone m_AudioZone;

	protected override void Awake()
	{
		base.Awake();
		m_AudioZone = GetComponentInParent<AudioZone>();
	}

	public override void OnTrigger()
	{
		if (DelayEnterExit && HaveAnyTriggers(TriggerType.ZoneEntered | TriggerType.ZoneExited) && (bool)m_AudioZone)
		{
			m_AudioZone.ExclusiveDelayedAction(m_DelaySeconds, SetState);
		}
		else
		{
			SetState();
		}
	}

	private void SetState()
	{
		m_State.Set();
	}
}
