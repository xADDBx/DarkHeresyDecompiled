using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[ComponentName("Heal/HealthLevelTrigger")]
[AllowMultipleComponents]
[TypeId("d74c5a91c6b41cc4292e249970fa7c49")]
public class HealthLevelTrigger : MechanicEntityFactComponentDelegate, IDamageHandler, ISubscriber
{
	[SerializeField]
	private RestrictionCalculator m_Restrictions = new RestrictionCalculator();

	[HideIf("UseValueInstead")]
	public int Percentage;

	[ShowIf("UseValueInstead")]
	public int Value;

	public bool UseValueInstead;

	public ActionList Actions;

	public void HandleDamageDealt(RuleDealDamage dealDamage)
	{
		using (ContextData<SavableTriggerData>.Request().Setup(base.ExecutesCount))
		{
			if (!m_Restrictions.IsPassed(base.Context, null, null, dealDamage))
			{
				return;
			}
		}
		if (ShouldTriggerHealthLevel(dealDamage))
		{
			base.Fact.RunActionInContext(Actions, (TargetWrapper)dealDamage.Target);
			base.ExecutesCount++;
		}
	}

	private bool ShouldTriggerHealthLevel(RuleDealDamage dealDamage)
	{
		PartHealth healthOptional = dealDamage.Target.GetHealthOptional();
		if (base.Owner != dealDamage.Target || healthOptional == null)
		{
			return false;
		}
		int num = (UseValueInstead ? Value : ((int)(0.01 * (double)Percentage * (double)healthOptional.MaxHitPoints)));
		int hitPointsLeft = healthOptional.HitPointsLeft;
		bool flag = hitPointsLeft + dealDamage.ResultValue > num;
		bool flag2 = hitPointsLeft <= num;
		return flag && flag2;
	}
}
