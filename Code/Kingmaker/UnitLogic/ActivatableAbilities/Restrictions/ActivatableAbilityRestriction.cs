using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;

[Obsolete]
[AllowedOn(typeof(BlueprintActivatableAbility))]
[TypeId("4fc45af926b34ff49be18aa442ec3aff")]
public abstract class ActivatableAbilityRestriction : UnitFactComponentDelegate
{
	protected new ActivatableAbility Fact => (ActivatableAbility)base.Fact;

	public bool IsAvailable(EntityFactComponent runtime)
	{
		using (runtime.SetScope())
		{
			return IsAvailable();
		}
	}

	protected abstract bool IsAvailable();
}
