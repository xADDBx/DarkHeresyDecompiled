using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Components;

[Serializable]
[ComponentName("LoS and Covers/ProvidesCover")]
[TypeId("a90f9dc74a764ec7977b09b37ec68795")]
public sealed class ProvidesCover : UnitFactComponentDelegate
{
	protected override void OnActivateOrPostLoad()
	{
		base.Owner.GetOrCreate<PartProvidesCover>().Retain(this);
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetOptional<PartProvidesCover>()?.Release(this);
	}
}
