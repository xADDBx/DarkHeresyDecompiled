using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[Serializable]
[AllowedOn(typeof(BlueprintUnitFact))]
[ComponentName("Psyker/Psyker")]
[TypeId("b27e65e0440f49409a5f19da2483cd1e")]
public class Psyker : UnitFactComponentDelegate
{
	protected override void OnActivateOrPostLoad()
	{
		base.Owner.Parts.GetOrCreate<PartPsyker>().Retain();
	}

	protected override void OnDeactivate()
	{
		base.Owner.Parts.GetOptional<PartPsyker>()?.Release();
	}
}
