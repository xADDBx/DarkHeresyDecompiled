using Kingmaker.Code.Framework;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.Framework;

public class ToggleAbilityCollection : MechanicEntityFactsCollection<ToggleAbility>
{
	protected override ToggleAbility PrepareFactForAttach(ToggleAbility fact)
	{
		return fact;
	}

	protected override ToggleAbility PrepareFactForDetach(ToggleAbility fact)
	{
		return fact;
	}
}
