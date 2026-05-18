using System;
using Kingmaker.ElementsSystem;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.Framework.DetectiveSystem;

[Serializable]
[TypeId("5f27f28124f64272a46fc2b0804ac06e")]
public sealed class HasClue : Condition, IHasDetectiveCaseItemCondition
{
	[ValidateNotNull]
	public BpRef<BlueprintClue> Clue;

	[InfoBox("Не учитывать то, что полученно после закрытия дела")]
	public bool ExcludeHidden;

	BlueprintCaseItem IHasDetectiveCaseItemCondition.CaseItem => (BlueprintClue?)Clue;

	bool IHasDetectiveCaseItemCondition.Not => Not;

	protected override string GetConditionCaption()
	{
		return $"Has clue {Clue}";
	}

	protected override bool CheckCondition()
	{
		return Game.Instance.DetectiveSystem.HasItem((BlueprintClue?)Clue, ExcludeHidden);
	}
}
