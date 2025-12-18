using System;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Framework.DetectiveSystem;

[Serializable]
[TypeId("2a82b1dd320e43708adb5d5419b67ca7")]
public sealed class AddClue : GameAction
{
	[ValidateNotNull]
	public BpRef<BlueprintClue> Clue;

	public BpRef<BlueprintScriptableObject> Source;

	public override string GetCaption()
	{
		return $"Add clue {Clue} (source: {Source})";
	}

	protected override void RunAction()
	{
		Game.Instance.DetectiveSystem.AddClue(Clue, Source);
	}
}
