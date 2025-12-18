using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.View.Covers;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;

namespace Owlcat.AI.Mechanics.Positioning;

[Serializable]
[ComponentName("AI/AiPositioningCanUseAbilityOnTargetGetter")]
[TypeId("a22230adebdc4547baede43d73c7e6af")]
public class AiPositioningCanUseAbilityOnTargetGetter : BoolPropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	public BlueprintAbilityReference Ability;

	public PropertyTargetType Target;

	protected override bool GetBaseValue()
	{
		Ability ability = (base.CurrentEntity as BaseUnitEntity)?.Abilities.GetAbility(Ability);
		if (ability == null)
		{
			return false;
		}
		MechanicEntity targetByType = this.GetTargetByType(Target);
		GridNodeBase casterNode = AiPositioningData.CurrentNode as GridNodeBase;
		int distance;
		LosCalculations.CoverType los;
		return ability.Data.CanTargetFromNode(casterNode, null, targetByType, out distance, out los);
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Can " + FormulaTargetScope.Current + " use " + Ability.NameSafe() + " on " + Target.Colorized() + " from <color='purple'>graph node</color>";
	}
}
