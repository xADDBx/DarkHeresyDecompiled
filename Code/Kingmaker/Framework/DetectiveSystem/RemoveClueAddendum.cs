using System;
using Kingmaker.ElementsSystem;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Framework.DetectiveSystem;

[Serializable]
[TypeId("f3e43a0aded34420bd78f341eccff8ed")]
public sealed class RemoveClueAddendum : GameAction
{
	[ValidateNotNull]
	public BpRef<BlueprintClueAddendum> Addendum;

	public override string GetCaption()
	{
		return $"Remove clue addendum {Addendum}";
	}

	protected override void RunAction()
	{
		Game.Instance.DetectiveSystem.RemoveAddendum(Addendum);
	}
}
