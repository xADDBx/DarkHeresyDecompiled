using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("22f8af664e0f421b81baa98cd36b8b69")]
public class ForbidRotation : UnitFactComponentDelegate
{
	protected override void OnActivateOrPostLoad()
	{
		base.Owner.Features.RotationForbidden.Retain();
	}

	protected override void OnDeactivate()
	{
		base.Owner.Features.RotationForbidden.Release();
	}
}
