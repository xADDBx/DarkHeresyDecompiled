using System;
using Kingmaker.ElementsSystem;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.Framework.DetectiveSystem;

[Serializable]
[TypeId("e62227e02dad46099d8b4f6fc3db54bb")]
public sealed class HasConclusion : Condition, IHasDetectiveCaseItemCondition
{
	[ValidateNotNull]
	public BpRef<BlueprintConclusion> Conclusion;

	[InfoBox("Не учитывать то, что полученно после закрытия дела")]
	public bool ExcludeHidden;

	BlueprintCaseItem IHasDetectiveCaseItemCondition.CaseItem => (BlueprintConclusion?)Conclusion;

	bool IHasDetectiveCaseItemCondition.Not => Not;

	protected override string GetConditionCaption()
	{
		return $"Has conclusion {Conclusion}";
	}

	protected override bool CheckCondition()
	{
		return Game.Instance.DetectiveSystem.HasItem((BlueprintConclusion?)Conclusion, ExcludeHidden);
	}
}
