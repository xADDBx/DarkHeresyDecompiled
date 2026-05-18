using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("603ec7b842284a9ca2db7783ec9365ba")]
public class WarhammerHitChanceBonusTarget : UnitFactComponentDelegate, ITargetRulebookHandler<RuleCalculateHitChances>, IRulebookHandler<RuleCalculateHitChances>, ISubscriber, ITargetRulebookSubscriber
{
	public ContextValue Value;

	public bool SpecificRangeType;

	[ShowIf("SpecificRangeType")]
	public WeaponRangeType WeaponRangeType;

	public void OnEventAboutToTrigger(RuleCalculateHitChances evt)
	{
	}

	public void OnEventDidTrigger(RuleCalculateHitChances evt)
	{
	}
}
