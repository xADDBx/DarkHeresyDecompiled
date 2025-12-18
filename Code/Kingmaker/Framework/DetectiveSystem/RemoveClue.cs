using System;
using Kingmaker.ElementsSystem;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Framework.DetectiveSystem;

[Serializable]
[TypeId("658dae18f7c34546a3fbc7cce27ee941")]
public sealed class RemoveClue : GameAction
{
	[ValidateNotNull]
	public BpRef<BlueprintClue> Clue;

	public override string GetCaption()
	{
		return $"Remove clue {Clue}";
	}

	protected override void RunAction()
	{
		Game.Instance.DetectiveSystem.RemoveClue(Clue);
	}
}
