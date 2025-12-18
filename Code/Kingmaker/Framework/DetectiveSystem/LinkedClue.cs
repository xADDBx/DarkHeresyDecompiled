using System;
using Kingmaker.ElementsSystem;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;

namespace Kingmaker.Framework.DetectiveSystem;

[Serializable]
public sealed class LinkedClue
{
	public ConditionsChecker UnlockCondition = new ConditionsChecker();

	[ValidateNotNull]
	public BpRef<BlueprintClue> Clue;
}
