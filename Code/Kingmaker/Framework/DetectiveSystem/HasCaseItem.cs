using System;
using Kingmaker.ElementsSystem;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Framework.DetectiveSystem;

[Serializable]
[TypeId("82b1eec3fbfe41b8bdc60895a034f444")]
public sealed class HasCaseItem : Condition, IHasDetectiveCaseItemCondition
{
	[ValidateNotNull]
	public BpRef<BlueprintCaseItem> CaseItem;

	BlueprintCaseItem IHasDetectiveCaseItemCondition.CaseItem => CaseItem;

	bool IHasDetectiveCaseItemCondition.Not => Not;

	protected override string GetConditionCaption()
	{
		return $"Has case item {CaseItem}";
	}

	protected override bool CheckCondition()
	{
		return Game.Instance.DetectiveSystem.HasItem(CaseItem);
	}
}
