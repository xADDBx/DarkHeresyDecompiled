using System.Collections.Generic;
using Code.Editor;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Localization;
using Kingmaker.UI.Sound;
using Owlcat.BehaviourTrees;
using UnityEngine;

namespace Owlcat.AI;

public class ShowBarkNode : ActionNode, IBarkSource
{
	public enum BarkDurationType
	{
		DefaultDuration,
		CustomDuration,
		DurationByText
	}

	private const float MinBarkDuration = 0.3f;

	private readonly EntityVariable m_Agent;

	private readonly SharedStringAsset m_BarkString;

	private readonly float m_Duration;

	public IEnumerable<LocalizedString> Barks => new LocalizedString[1] { m_BarkString.String };

	public bool IsVoIdForced => false;

	public List<string> ForcedVoGuids => null;

	public bool Spammable => false;

	public ShowBarkNode(EntityVariable agent, SharedStringAsset barkString, BarkDurationType durationType, float customDuration)
	{
		m_Agent = agent;
		m_BarkString = barkString;
		m_Duration = durationType switch
		{
			BarkDurationType.CustomDuration => Mathf.Max(0.3f, customDuration), 
			BarkDurationType.DurationByText => UtilityBark.GetBarkDuration(m_BarkString.String), 
			_ => UtilityBark.DefaultBarkTime, 
		};
	}

	protected override void DoAction()
	{
		string voGuidBySourceAndTarget = VoiceOverController.GetVoGuidBySourceAndTarget(this, m_Agent.Value);
		BarkPlayer.Bark(m_Agent.Value, m_BarkString.String, VoiceOverType.Bark, voGuidBySourceAndTarget, m_Duration);
	}
}
