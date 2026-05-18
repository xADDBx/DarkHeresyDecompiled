using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Gameplay.Blueprints.Root.Strings;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;

[AllowedOn(typeof(BlueprintAbility))]
[AllowedOn(typeof(BlueprintToggleAbility))]
[AllowMultipleComponents]
[ComponentName("Caster Restriction/WarhammerAbilityManageResources")]
[TypeId("6667ad881b0ea6c4fab5ef5b77490081")]
public class WarhammerAbilityManageResources : BlueprintComponent, IAbilityCasterRestriction, IAbilityOnCastLogic
{
	public bool CostsMaximumMovePoints;

	[HideIf("CostsMaximumMovePoints")]
	public int mustHaveMovePoints;

	[HideIf("CostsMaximumMovePoints")]
	public int shouldSpendMovePoints;

	public bool IsCasterRestrictionPassed(MechanicEntity caster)
	{
		PartUnitCombatState combatStateOptional = caster.GetCombatStateOptional();
		if (combatStateOptional == null)
		{
			return true;
		}
		int num = (CostsMaximumMovePoints ? combatStateOptional.MovementPointsMax : mustHaveMovePoints);
		return combatStateOptional.MovementPoints >= (float)num;
	}

	public string GetAbilityCasterRestrictionUIText(MechanicEntity caster)
	{
		return LocalizedTexts.Instance.Reasons.NoResources;
	}

	public IEnumerable<string> GetAbilityCasterRestrictionShortUITexts(MechanicEntity caster)
	{
		CasterRestrictionsStrings casterRestrictionsStrings = ConfigRoot.Instance.LocalizedTexts.CasterRestrictionsStrings;
		yield return IsCasterRestrictionPassed(caster) ? casterRestrictionsStrings.MovePointsSufficient : casterRestrictionsStrings.MovePointsInsufficient;
	}

	public void OnCast(AbilityExecutionContext context)
	{
		PartUnitCombatState combatStateOptional = context.Caster.GetCombatStateOptional();
		if (combatStateOptional != null)
		{
			int num = (CostsMaximumMovePoints ? combatStateOptional.MovementPointsMax : shouldSpendMovePoints);
			combatStateOptional.SpendMovementPoints(num);
		}
	}
}
