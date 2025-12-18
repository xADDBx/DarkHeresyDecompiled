using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[ComponentName("Buff on spawned unit")]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("efb3e00290a25e543a923bfc256c22ee")]
[AllowMultipleComponents]
public class OnSpawnBuff : UnitFactComponentDelegate, IInitiatorRulebookHandler<RulePerformSummonUnit>, IRulebookHandler<RulePerformSummonUnit>, ISubscriber, IInitiatorRulebookSubscriber
{
	[InfoBox("Caster's Fact")]
	[SerializeField]
	[FormerlySerializedAs("IfHaveFact")]
	private BlueprintFeatureReference m_IfHaveFact;

	public bool CheckSummonedUnitFact;

	[ShowIf("CheckSummonedUnitFact")]
	[SerializeField]
	private BlueprintFeatureReference m_IfSummonHaveFact;

	[SerializeField]
	[FormerlySerializedAs("buff")]
	private BlueprintBuffReference m_buff;

	public bool CheckDescriptor;

	[ShowIf("CheckDescriptor")]
	public SpellDescriptorWrapper SpellDescriptor;

	public bool IsInfinity;

	[HideIf("IsInfinity")]
	public Rounds duration;

	public BlueprintFeature IfHaveFact => m_IfHaveFact?.Get();

	public BlueprintFeature IfSummonHaveFact => m_IfSummonHaveFact?.Get();

	public BlueprintBuff buff => m_buff?.Get();

	public void OnEventAboutToTrigger(RulePerformSummonUnit evt)
	{
	}

	public void OnEventDidTrigger(RulePerformSummonUnit evt)
	{
	}
}
