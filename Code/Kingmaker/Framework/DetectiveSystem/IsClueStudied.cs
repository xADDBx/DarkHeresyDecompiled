using System;
using Kingmaker.ElementsSystem;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Framework.DetectiveSystem;

[Serializable]
[TypeId("83d3728e846d4e84b14350b83efd7c42")]
public sealed class IsClueStudied : Condition
{
	[ValidateNotNull]
	public BpRef<BlueprintClueStudy> Study;

	protected override string GetConditionCaption()
	{
		return $"Is {Study} studied";
	}

	protected override bool CheckCondition()
	{
		return Game.Instance.DetectiveSystem.IsStudied(Study);
	}
}
