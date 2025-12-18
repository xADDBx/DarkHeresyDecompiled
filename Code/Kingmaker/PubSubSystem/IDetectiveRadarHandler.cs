using Kingmaker.Code.Gameplay.Controllers.DetectiveRadar;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IDetectiveRadarHandler : ISubscriber
{
	void HandleRadarModeChange(DetectiveRadarState state);

	void HandleNearestSignalTurnedOn();
}
