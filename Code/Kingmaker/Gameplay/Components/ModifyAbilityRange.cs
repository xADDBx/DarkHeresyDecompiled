using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Components;

[Serializable]
[AllowMultipleComponents]
[ComponentName("Ability/ModifyAbilityRange")]
[TypeId("f317942662e642cda59237d70135a43f")]
public class ModifyAbilityRange : MechanicEntityFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateAbilityRange>, IRulebookHandler<RuleCalculateAbilityRange>, ISubscriber, IInitiatorRulebookSubscriber
{
	public enum ModificationType
	{
		Add,
		Override
	}

	public RestrictionCalculator Restrictions;

	public ModificationType Type;

	public ContextValue Range;

	public void OnEventAboutToTrigger(RuleCalculateAbilityRange evt)
	{
		if (Restrictions.IsPassed(base.Context, null, null, evt))
		{
			int num = Range.Calculate(base.Context);
			switch (Type)
			{
			case ModificationType.Add:
				evt.Bonus += num;
				break;
			case ModificationType.Override:
				evt.OverrideRange = num;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}

	public void OnEventDidTrigger(RuleCalculateAbilityRange evt)
	{
	}
}
