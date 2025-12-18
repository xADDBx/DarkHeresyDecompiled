using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Owlcat.AI.Mechanics.BodyParts;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.AI.Mechanics.Positioning;

[Serializable]
[ComponentName("AI/BodyPart/AiBodyPartHitChancesGetter")]
[TypeId("262c2186f2654428b98a8321edef498a")]
public class AiBodyPartHitChancesGetter : IntPropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	public BlueprintAbilityReference Ability;

	public PropertyTargetType Target;

	protected override int GetBaseValue()
	{
		BaseUnitEntity baseUnitEntity = base.CurrentEntity as BaseUnitEntity;
		MechanicEntity targetByType = this.GetTargetByType(Target);
		Ability ability = baseUnitEntity?.Abilities.GetAbility(Ability);
		BlueprintBodyPart currentBodyPart = AiBodyPartsContextData.CurrentBodyPart;
		if (ability == null || !targetByType.BodyParts.Contains(currentBodyPart))
		{
			return 0;
		}
		ability.Data.Clone().PreciseBodyPart = currentBodyPart;
		return Rulebook.Trigger(new RuleCalculateHitChances(baseUnitEntity, targetByType, ability.Data, 0)).ResultHitChance;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Chances to hit " + Target.Colorized() + "'s <color='purple'>body part</color> with " + Ability.NameSafe();
	}
}
