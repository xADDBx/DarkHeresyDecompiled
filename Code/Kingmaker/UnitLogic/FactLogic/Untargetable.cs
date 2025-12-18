using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[ComponentName("Target Restriction/Untargetable")]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("c96d8c2d5342c4f42848d3758aa21767")]
public class Untargetable : UnitFactComponentDelegate
{
	protected override void OnActivateOrPostLoad()
	{
		base.Owner.Features.IsUntargetable.Retain();
	}

	protected override void OnDeactivate()
	{
		base.Owner.Features.IsUntargetable.Release();
	}
}
