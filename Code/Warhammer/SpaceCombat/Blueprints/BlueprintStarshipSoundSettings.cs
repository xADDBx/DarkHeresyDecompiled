using System;
using Kingmaker.Blueprints;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Warhammer.SpaceCombat.Blueprints;

[Obsolete]
[TypeId("31037214c438fde4ea8ace268b5db905")]
public class BlueprintStarshipSoundSettings : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintStarshipSoundSettings>
	{
	}

	[SerializeField]
	[AkEventReference]
	private string m_PlayEvent;

	[SerializeField]
	[AkEventReference]
	private string m_StopEvent;

	[SerializeField]
	[AkSwitchGroupReference]
	private string m_SwitchGroupEngineStatus;

	[SerializeField]
	[AkGameParameterReference]
	private string m_RtpcMovementStatus;

	[SerializeField]
	[AkGameParameterReference]
	private string m_RtpcStarshipHealth;
}
