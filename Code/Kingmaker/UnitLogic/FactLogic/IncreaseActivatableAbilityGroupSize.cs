using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete]
[AllowMultipleComponents]
[TypeId("b771d1643e9f42f4db53457ba102f505")]
public class IncreaseActivatableAbilityGroupSize : UnitFactComponentDelegate
{
	public ActivatableAbilityGroup Group;

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.GetOrCreate<UnitPartActivatableAbility>().IncreaseGroupSize(Group);
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetOrCreate<UnitPartActivatableAbility>().DecreaseGroupSize(Group);
	}
}
