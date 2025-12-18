using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Visual.Sound;

public class DetectiveReminderAskController : BaseAsksController, ICloseLoadingScreenHandler, ISubscriber
{
	public void HandleCloseLoadingScreen()
	{
		TryScheduleAsk();
	}

	private void TryScheduleAsk()
	{
		if (!Game.Instance.EntityPools.MapObjects.Where((MapObjectEntity x) => x is DetectiveTraceEntity).Cast<DetectiveTraceEntity>().All((DetectiveTraceEntity e) => e.Status != DetectiveTraceStatus.Found))
		{
			PartDetectiveServoSkull.Find()?.Owner.View.Asks?.DetectiveReminder.Schedule();
		}
	}
}
