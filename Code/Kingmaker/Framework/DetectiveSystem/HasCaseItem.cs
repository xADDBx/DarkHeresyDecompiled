using System;
using Kingmaker.ElementsSystem;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.Framework.DetectiveSystem;

[Serializable]
[TypeId("82b1eec3fbfe41b8bdc60895a034f444")]
public sealed class HasCaseItem : Condition, IHasDetectiveCaseItemCondition
{
	[ValidateNotNull]
	public BpRef<BlueprintCaseItem> CaseItem;

	[InfoBox("Не учитывать то, что полученно после закрытия дела")]
	public bool ExcludeHidden;

	BlueprintCaseItem IHasDetectiveCaseItemCondition.CaseItem => CaseItem;

	bool IHasDetectiveCaseItemCondition.Not => Not;

	protected override string GetConditionCaption()
	{
		return $"Has case item {CaseItem}";
	}

	protected override bool CheckCondition()
	{
		return Game.Instance.DetectiveSystem.HasItem(CaseItem, ExcludeHidden);
	}
}
