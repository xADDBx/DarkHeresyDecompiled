using System;
using Kingmaker.ElementsSystem;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Framework.DetectiveSystem;

[Serializable]
[TypeId("e62227e02dad46099d8b4f6fc3db54bb")]
public sealed class HasConclusion : Condition, IHasDetectiveCaseItemCondition
{
	[ValidateNotNull]
	public BpRef<BlueprintConclusion> Conclusion;

	BlueprintCaseItem IHasDetectiveCaseItemCondition.CaseItem => (BlueprintConclusion?)Conclusion;

	bool IHasDetectiveCaseItemCondition.Not => Not;

	protected override string GetConditionCaption()
	{
		return $"Has conclusion {Conclusion}";
	}

	protected override bool CheckCondition()
	{
		return Game.Instance.DetectiveSystem.HasConclusion(Conclusion);
	}
}
