using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintUnitFact))]
[AllowedOn(typeof(BlueprintUnit))]
[TypeId("be0743d431384527bfeb78e6eb3173ed")]
public class AscensionParametersFactComponent : UnitBuffComponentDelegate
{
	protected override void OnActivate()
	{
		base.Context.MaybeCaster?.GetOrCreate<UnitPartAscensionParameters>();
	}
}
