using System;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull.Conditions;

[Serializable]
[TypeId("296c58b348694d14a2e65c778a0dfa61")]
public sealed class CheckDetectiveServoskullEnabled : Condition
{
	protected override string GetConditionCaption()
	{
		return "Detective servoskull enabled";
	}

	protected override bool CheckCondition()
	{
		return PartDetectiveServoSkull.Find()?.Enabled ?? false;
	}
}
