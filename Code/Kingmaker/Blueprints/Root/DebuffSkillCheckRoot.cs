using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
[ComponentName("Root/DebuffSkillCheckRoot")]
[TypeId("eaa1747159d24b8e883ca5c0b6429137")]
public class DebuffSkillCheckRoot : BlueprintScriptableObject
{
	[SerializeField]
	private BlueprintBuffReference m_Fatigued;

	[SerializeField]
	private BlueprintBuffReference m_Disturbed;

	[SerializeField]
	private BlueprintBuffReference m_Perplexed;

	public BlueprintBuff Fatigued => m_Fatigued;

	public BlueprintBuff Disturbed => m_Disturbed;

	public BlueprintBuff Perplexed => m_Perplexed;
}
