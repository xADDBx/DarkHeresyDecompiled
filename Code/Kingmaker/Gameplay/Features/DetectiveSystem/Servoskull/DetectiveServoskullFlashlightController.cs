using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Interfaces;

namespace Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;

public sealed class DetectiveServoskullFlashlightController : IControllerTick, IController
{
	private static DetectiveServoskullRoot Settings => ConfigRoot.Instance.DetectiveServoskull;

	TickType IControllerTick.GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
	}
}
