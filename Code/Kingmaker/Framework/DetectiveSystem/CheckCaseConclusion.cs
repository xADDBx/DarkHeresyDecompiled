using System;
using Kingmaker.ElementsSystem;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Framework.DetectiveSystem;

[Serializable]
[Obsolete("New Question/Answer approach, WIP")]
[TypeId("0bc3a3fcb841409bbd109aa43f150456")]
public sealed class CheckCaseConclusion : Condition
{
	[ValidateNotNull]
	public BpRef<BlueprintCase> Case;

	[ValidateNotNull]
	public BpRef<BlueprintConclusion> Conclusion;

	protected override string GetConditionCaption()
	{
		return $"Check case {Case} is closed with conclusion {Conclusion}";
	}

	protected override bool CheckCondition()
	{
		return Game.Instance.DetectiveSystem.GetCaseConclusions(Case)?.HasItem(Conclusion) ?? false;
	}
}
