using System;
using System.Threading.Tasks;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework.Interaction;
using Kingmaker.Mechanics.Entities;

namespace Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;

[Serializable]
public sealed class ServoSkullScanInteractionModule : InteractionModule
{
	public override string GetCaption()
	{
		return "Servo-skull scan";
	}

	public override async Task Execute(BaseUnitEntity initiator, MapObjectEntity target)
	{
		PartDetectiveServoSkull fromOwner = PartDetectiveServoSkull.GetFromOwner(initiator);
		if (fromOwner != null)
		{
			await fromOwner.Scan(target);
		}
	}

	public override bool CanInteract(MapObjectEntity owner)
	{
		return !(PartDetectiveServoSkull.Find()?.IsBusy ?? false);
	}

	public override bool CanBeSelected(BaseUnitEntity unit)
	{
		PartDetectiveServoSkull fromOwner = PartDetectiveServoSkull.GetFromOwner(unit);
		if (fromOwner != null)
		{
			return !fromOwner.IsBusy;
		}
		return false;
	}

	public override AbstractUnitEntity? GetProcessUser(BaseUnitEntity initiator)
	{
		return PartDetectiveServoSkull.GetFromOwner(initiator)?.Owner;
	}
}
