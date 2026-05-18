using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Serializable]
[TypeId("bfe787e51845449dbd98425f134a555b")]
public class ContextActionAddBonusAbilityUsage : ContextAction
{
	[SerializeField]
	[CanBeNull]
	private RestrictionsHolder.Reference m_Restriction;

	[SerializeField]
	private bool m_IgnoreAbilityRestrictionForUsage;

	[SerializeField]
	private PropertyCalculator m_Count;

	[SerializeField]
	private PropertyCalculator m_CostBonus;

	[SerializeField]
	private bool m_ToTarget;

	private static LogChannel Logger => BonusAbilityExtension.Logger;

	public override string GetCaption()
	{
		if (!m_ToTarget)
		{
			return "Add bonus ability usage to caster";
		}
		return "Add bonus ability usage to target";
	}

	protected override void RunAction()
	{
		MechanicEntity mechanicEntity = (m_ToTarget ? base.Target.Entity : base.Context.Caster);
		if (mechanicEntity == null)
		{
			Logger.Error(this, "Unable to add bonus ability usage: target is null");
			return;
		}
		if (!(mechanicEntity is BaseUnitEntity baseUnitEntity))
		{
			Logger.Error(this, "Unable to add bonus ability usage: target is not BaseUnitEntity");
			return;
		}
		MechanicEntity currentEntity = (MechanicEntity)(((object)base.Context.AreaEffect) ?? ((object)baseUnitEntity));
		EntityFactSource source = GetSource(baseUnitEntity);
		int value = m_Count.GetValue(currentEntity, base.Context);
		int value2 = m_CostBonus.GetValue(currentEntity, base.Context);
		baseUnitEntity.GetOrCreate<UnitPartBonusAbility>().AddBonusAbility(source, value, value2, m_Restriction, m_IgnoreAbilityRestrictionForUsage);
	}

	private EntityFactSource GetSource(BaseUnitEntity unit)
	{
		AreaEffectEntity areaEffect = base.Context.AreaEffect;
		if (areaEffect != null)
		{
			return new EntityFactSource(areaEffect);
		}
		AbilityExecutionContext abilityContext = base.AbilityContext;
		if (abilityContext != null)
		{
			BlueprintAbility abilityBlueprint = abilityContext.AbilityBlueprint;
			if (abilityBlueprint != null)
			{
				return new EntityFactSource(abilityBlueprint);
			}
		}
		if (base.Context.Blueprint != null)
		{
			return new EntityFactSource(base.Context.Blueprint);
		}
		return new EntityFactSource(unit);
	}
}
