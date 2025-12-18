using System;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;

namespace Kingmaker.Framework.DetectiveSystem;

[Serializable]
[Obsolete]
public sealed class UnlockCondition
{
	public enum ConditionType
	{
		All,
		Any
	}

	public ConditionType Type;

	[ValidateNoNullEntries]
	public BpRef<BlueprintCaseItem>[] RequiredItems = new BpRef<BlueprintCaseItem>[0];
}
