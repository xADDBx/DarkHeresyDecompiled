using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic.Enums;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete]
[ComponentName("Add condition")]
[AllowedOn(typeof(BlueprintUnitFact))]
[AllowedOn(typeof(BlueprintUnit))]
[AllowMultipleComponents]
[TypeId("4c36aacebf153574eb39757fc3965edb")]
public class AddCondition : UnitFactComponentDelegate
{
	public UnitCondition Condition;

	protected override void OnActivateOrPostLoad()
	{
	}

	protected override void OnDeactivate()
	{
	}
}
