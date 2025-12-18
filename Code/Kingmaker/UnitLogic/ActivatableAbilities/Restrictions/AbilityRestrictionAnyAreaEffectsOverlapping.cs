using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;

[Obsolete]
[TypeId("25c565ca84244ffd94ad766ff6fdf7da")]
public class AbilityRestrictionAnyAreaEffectsOverlapping : BlueprintComponent, IAbilityPatternRestriction
{
	public bool IsPatternRestrictionPassed(AbilityData ability, MechanicEntity caster, TargetWrapper target)
	{
		OrientedPatternData orientedPattern = ability.GetPatternSettings().GetOrientedPattern(ability, ability.Caster.GetNearestNodeXZ(), target.NearestNode);
		foreach (AreaEffectEntity areaEffect in Game.Instance.EntityPools.AreaEffects)
		{
			if (areaEffect.Blueprint.SavePersistentArea && areaEffect.Overlaps(orientedPattern.Nodes))
			{
				return false;
			}
		}
		return true;
	}

	public string GetAbilityPatternRestrictionUIText(AbilityData ability, MechanicEntity caster, TargetWrapper target)
	{
		return LocalizedTexts.Instance.Reasons.AreaEffectsCannotOverlap;
	}
}
