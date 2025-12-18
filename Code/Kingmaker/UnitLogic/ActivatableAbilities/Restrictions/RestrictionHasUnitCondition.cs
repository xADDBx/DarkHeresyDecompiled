using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Enums;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;

[Obsolete]
[AllowMultipleComponents]
[ComponentName("AA restriction unit condition")]
[TypeId("8d32cf270a3f8df438c65ac197fca2d2")]
public class RestrictionHasUnitCondition : ActivatableAbilityRestriction
{
	public UnitCondition Condition;

	public bool Invert;

	protected override bool IsAvailable()
	{
		return false;
	}
}
