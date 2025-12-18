using System;
using Kingmaker.ElementsSystem;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Framework.DetectiveSystem;

[Serializable]
[TypeId("9f57f3fa04c34e56b543bf318a188e78")]
public sealed class ReopenCase : GameAction
{
	[ValidateNotNull]
	public BpRef<BlueprintCase> Case;

	public override string GetCaption()
	{
		return $"Reopen case {Case}";
	}

	protected override void RunAction()
	{
		Game.Instance.DetectiveSystem.ReopenCase(Case);
	}
}
