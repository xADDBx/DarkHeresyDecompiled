using System;
using Kingmaker.ElementsSystem;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Framework.DetectiveSystem;

[Serializable]
[TypeId("5d1de7475be44d4da810ad1b9e2e4c1e")]
public sealed class OpenCase : GameAction
{
	[ValidateNotNull]
	public BpRef<BlueprintCase> Case;

	public override string GetCaption()
	{
		return $"Open case {Case}";
	}

	protected override void RunAction()
	{
		Game.Instance.DetectiveSystem.OpenCase(Case);
	}
}
