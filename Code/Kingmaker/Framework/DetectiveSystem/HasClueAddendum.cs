using System;
using Kingmaker.ElementsSystem;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.Framework.DetectiveSystem;

[Serializable]
[TypeId("2f8c2e8af72c402abaa744947b0ca232")]
public sealed class HasClueAddendum : Condition, IHasDetectiveCaseItemCondition
{
	[ValidateNotNull]
	public BpRef<BlueprintClueAddendum> Addendum;

	[InfoBox("Не учитывать то, что полученно после закрытия дела")]
	public bool ExcludeHidden;

	BlueprintCaseItem IHasDetectiveCaseItemCondition.CaseItem => (BlueprintClueAddendum?)Addendum;

	bool IHasDetectiveCaseItemCondition.Not => Not;

	protected override string GetConditionCaption()
	{
		return $"Has clue addendum {Addendum}";
	}

	protected override bool CheckCondition()
	{
		return Game.Instance.DetectiveSystem.HasItem((BlueprintClueAddendum?)Addendum, ExcludeHidden);
	}
}
