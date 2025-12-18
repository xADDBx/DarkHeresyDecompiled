using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.Blueprints;

[Obsolete]
[AllowedOn(typeof(BlueprintUnit))]
[ClassInfoBox("Determines if unit should aggro on other unit")]
[TypeId("262e96b05153e454da3f5027ef121ea5")]
public class UnitAggroFilter : BlueprintComponent
{
	[InfoBox("No conditions means always should aggro. Otherwise aggroes only if FilterCondition is true")]
	public ConditionsChecker FilterCondition;

	public ActionList ActionsOnAggro;

	public void OnAggroAction(BaseUnitEntity target, BaseUnitEntity attacker)
	{
	}

	public bool ShouldAggro(BaseUnitEntity target, BaseUnitEntity attacker)
	{
		return false;
	}
}
