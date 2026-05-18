using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Designers.Mechanics.Facts.Damage;
using Kingmaker.Framework;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;

namespace Kingmaker.Code.UI.MVVM.UnitInfo;

public class CasterDamageBuffsInfoVM : CasterBuffsInfoBaseVM<UnitBuffUIInfo>
{
	protected override void AddBuffIfRelevant(Buff buff, IEvalContext abilityContext, ICollection<UnitBuffUIInfo> buffInfos)
	{
		BlueprintBuff blueprint = buff.Blueprint;
		DamageModifierInitiator damageModifierInitiator = null;
		BlueprintComponent[] componentsArray = blueprint.ComponentsArray;
		for (int i = 0; i < componentsArray.Length; i++)
		{
			if (componentsArray[i] is DamageModifierInitiator damageModifierInitiator2 && damageModifierInitiator2.Restrictions.IsPassed(abilityContext))
			{
				damageModifierInitiator = damageModifierInitiator2;
				break;
			}
		}
		if (damageModifierInitiator != null)
		{
			AddModifier(damageModifierInitiator.Damage, buff, buffInfos);
			AddModifier(damageModifierInitiator.VitalDamage, buff, buffInfos);
			AddModifier(damageModifierInitiator.ArmorDamage, buff, buffInfos);
			AddModifier(damageModifierInitiator.HealthDamage, buff, buffInfos);
		}
	}

	private void AddModifier(ContextValueModifierWithType modifier, Buff buff, ICollection<UnitBuffUIInfo> buffInfos)
	{
		if (modifier.Enabled)
		{
			int num = modifier.Calculate(buff.Context);
			if (num >= 0)
			{
				buffInfos.Add(new UnitBuffUIInfo
				{
					Name = buff.Name,
					Icon = buff.Icon,
					ModifierType = modifier.ModifierType,
					Value = num
				});
			}
		}
	}
}
