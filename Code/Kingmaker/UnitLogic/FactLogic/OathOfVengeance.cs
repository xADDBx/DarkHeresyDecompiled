using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintUnitFact))]
[AllowedOn(typeof(BlueprintUnit))]
[TypeId("a22c234909574446b39d4cbb03c05ed0")]
public class OathOfVengeance : UnitFactComponentDelegate
{
	protected override void OnActivate()
	{
		base.Owner.GetOrCreate<UnitPartOathOfVengeance>();
	}
}
