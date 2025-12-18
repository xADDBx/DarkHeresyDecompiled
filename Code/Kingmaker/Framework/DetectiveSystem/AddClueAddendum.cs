using System;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Framework.DetectiveSystem;

[Serializable]
[TypeId("d9aeadc0d4174e39a661a8f86f34015f")]
public sealed class AddClueAddendum : GameAction
{
	[ValidateNotNull]
	public BpRef<BlueprintClueAddendum> Addendum;

	public BpRef<BlueprintScriptableObject> Source;

	public override string GetCaption()
	{
		return $"Add clue addendum {Addendum} (source: {Source})";
	}

	protected override void RunAction()
	{
		Game.Instance.DetectiveSystem.AddAddendum(Addendum, Source);
	}
}
