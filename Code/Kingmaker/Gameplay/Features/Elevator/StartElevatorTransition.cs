using System;
using Kingmaker.Blueprints;
using Kingmaker.Code.Framework.CutsceneSystem;
using Kingmaker.ElementsSystem;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Features.Elevator;

[Serializable]
[TypeId("1179e497bf39499ea829cdad1c1ded1d")]
public sealed class StartElevatorTransition : GameAction
{
	[AllowedEntityType(typeof(ElevatorPlatformView))]
	public EntityReference Elevator = new EntityReference();

	[AllowedEntityType(typeof(ElevatorPlatformStopView))]
	public EntityReference Destination = new EntityReference();

	public BpRef<BlueprintCutscene> CustomTransitionCutscene = new BpRef<BlueprintCutscene>();

	public override string GetCaption()
	{
		return $"Move elevator {Elevator} to {Destination}";
	}

	protected override void RunAction()
	{
		ElevatorPlatformEntity obj = (ElevatorPlatformEntity)(Elevator.FindData() ?? throw new NullReferenceException());
		ElevatorPlatformStopEntity destination = (ElevatorPlatformStopEntity)(Destination.FindData() ?? throw new NullReferenceException());
		obj.StartTransition(destination, CustomTransitionCutscene);
	}
}
