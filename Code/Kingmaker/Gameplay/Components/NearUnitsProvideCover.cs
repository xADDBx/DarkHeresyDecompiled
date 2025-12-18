using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.Framework.Abilities.Components;
using Kingmaker.Gameplay.Parts;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Components;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Components;

[Serializable]
[ComponentName("LoS and Covers/NearUnitsProvideCover")]
[TypeId("e34ee07a2f5d4f528dc70c39f011eee0")]
public sealed class NearUnitsProvideCover : UnitFactComponentDelegate, IRestrictionProvider
{
	public TargetType Filter;

	public RestrictionCalculator Restriction;

	RestrictionCalculator IRestrictionProvider.GetRestriction()
	{
		return Restriction;
	}

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.GetOrCreate<PartNearUnitsProvideCover>().Add(Filter, base.Fact, this);
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetOptional<PartNearUnitsProvideCover>()?.Remove(base.Fact, this);
	}
}
