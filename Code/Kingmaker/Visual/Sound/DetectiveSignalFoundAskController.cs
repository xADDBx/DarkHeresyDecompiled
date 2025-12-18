using Kingmaker.Code.Gameplay.Controllers.DetectiveRadar;
using Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Visual.Sound;

public class DetectiveSignalFoundAskController : BaseAsksController, IDetectiveRadarHandler, ISubscriber
{
	public void HandleRadarModeChange(DetectiveRadarState state)
	{
	}

	public void HandleNearestSignalTurnedOn()
	{
		ScheduleAsk();
	}

	private void ScheduleAsk()
	{
		PartDetectiveServoSkull.Find()?.Owner.View.Asks?.DetectiveSignalFound.Schedule();
	}
}
