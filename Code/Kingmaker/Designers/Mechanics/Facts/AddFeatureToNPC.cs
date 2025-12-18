using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("26c897e01b0440f4f93773579388a6c7")]
public class AddFeatureToNPC : UnitFactComponentDelegate
{
	public bool CheckParty;

	[SerializeField]
	[FormerlySerializedAs("Feature")]
	private BlueprintFeatureReference m_Feature;

	public bool CheckSummoned;

	public BlueprintFeature Feature => m_Feature?.Get();

	protected override void OnActivate()
	{
		if (base.Owner.Facts.FindBySource(Feature, base.Fact, this) == null)
		{
			RulePerformSummonUnit rulePerformSummonUnit = Rulebook.CurrentContext.LastEventOfType<RulePerformSummonUnit>();
			bool flag = rulePerformSummonUnit != null && CheckSummoned && base.Owner.Blueprint == rulePerformSummonUnit.Blueprint;
			bool flag2 = flag && ((rulePerformSummonUnit.Initiator.IsPlayerFaction && CheckParty) || (!rulePerformSummonUnit.Initiator.IsPlayerFaction && !CheckParty));
			if ((((CheckParty && base.Owner.Faction.IsPlayer) || (!CheckParty && !base.Owner.Faction.IsPlayer)) && (!CheckSummoned || !flag)) || flag2)
			{
				base.Owner.AddFact(Feature).AddSource(base.Fact, this);
			}
		}
	}

	protected override void OnDeactivate()
	{
		RemoveAllFactsOriginatedFromThisComponent(base.Owner);
	}
}
