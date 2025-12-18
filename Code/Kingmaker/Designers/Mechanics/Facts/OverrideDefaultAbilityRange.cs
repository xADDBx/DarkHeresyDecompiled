using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("62914d9ab9fe4dada246891867955ddd")]
public class OverrideDefaultAbilityRange : MechanicEntityFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateAbilityRange>, IRulebookHandler<RuleCalculateAbilityRange>, ISubscriber, IInitiatorRulebookSubscriber
{
	[SerializeField]
	private RestrictionCalculator m_Restrictions = new RestrictionCalculator();

	[SerializeField]
	private PropertyCalculator m_Range;

	public void OnEventAboutToTrigger(RuleCalculateAbilityRange evt)
	{
		if (m_Restrictions.IsPassed(base.Context, null, null, evt) && evt.Ability.Blueprint.SameAbility(base.OwnerBlueprint as BlueprintAbility))
		{
			if (m_Range == null)
			{
				PFLog.Default.Error("Range calculator is missing");
			}
			else
			{
				evt.OverrideRange = m_Range.GetValue(new PropertyContext(evt.ConcreteInitiator, base.Context, null, evt));
			}
		}
	}

	public void OnEventDidTrigger(RuleCalculateAbilityRange evt)
	{
	}
}
