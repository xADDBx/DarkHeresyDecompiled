using System;
using Kingmaker.ElementsSystem;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Features.Encounter.Actions;

[Serializable]
[TypeId("d634ea7ad9374e3d9cadca694e48aee1")]
public sealed class StartEncounter : GameAction
{
	[ValidateNotNull]
	public BpRef<BlueprintEncounter> Encounter = new BpRef<BlueprintEncounter>();

	public bool AmbushPlayer;

	public override string GetCaption()
	{
		return $"Start encounter {Encounter}";
	}

	protected override void RunAction()
	{
		ActiveEncounter.Start(Encounter, AmbushPlayer);
	}
}
