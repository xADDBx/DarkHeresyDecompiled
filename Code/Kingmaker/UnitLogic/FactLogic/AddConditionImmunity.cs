using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic.Enums;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete]
[ComponentName("Add condition immunity")]
[AllowedOn(typeof(BlueprintUnitFact))]
[AllowMultipleComponents]
[TypeId("c90fcf2050a28654c8d7dae6e90e204b")]
public class AddConditionImmunity : UnitFactComponentDelegate
{
	public UnitCondition Condition;

	protected override void OnActivateOrPostLoad()
	{
	}

	protected override void OnDeactivate()
	{
	}
}
