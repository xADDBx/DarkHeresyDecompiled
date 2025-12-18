using System;
using Kingmaker.ElementsSystem;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Framework.DetectiveSystem;

[Serializable]
[TypeId("4d2b1dee4af349908c2a236f5c594209")]
public sealed class TryAddConclusion : GameAction
{
	public BpRef<BlueprintConclusion> Conclusion;

	public override string GetCaption()
	{
		return $"Try add conclusion {Conclusion} with dependencies";
	}

	protected override void RunAction()
	{
		Game.Instance.DetectiveSystem.TryAddConclusionWithDependencies(Conclusion);
	}
}
