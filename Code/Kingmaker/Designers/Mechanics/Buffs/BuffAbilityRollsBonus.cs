using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Buffs;

[Obsolete]
[ComponentName("BuffMechanics/Ability Roll Bonus")]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("e8255e5a137d50245853bf6b21665cdc")]
public class BuffAbilityRollsBonus : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateSkillCheck>, IRulebookHandler<RuleCalculateSkillCheck>, ISubscriber, IInitiatorRulebookSubscriber
{
	public int Value;

	public ModifierDescriptor Descriptor;

	public bool AffectAllStats;

	public bool OnlyHighesStats;

	public ContextValue Multiplier;

	[ShowIf("ShowStatType")]
	public StatType Stat;

	[UsedImplicitly]
	private bool ShowStatType => !AffectAllStats;

	void IRulebookHandler<RuleCalculateSkillCheck>.OnEventAboutToTrigger(RuleCalculateSkillCheck evt)
	{
	}

	void IRulebookHandler<RuleCalculateSkillCheck>.OnEventDidTrigger(RuleCalculateSkillCheck evt)
	{
	}
}
