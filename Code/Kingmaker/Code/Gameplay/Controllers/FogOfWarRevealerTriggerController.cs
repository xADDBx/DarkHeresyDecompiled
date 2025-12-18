using Kingmaker.Controllers;
using Kingmaker.Controllers.Interfaces;

namespace Kingmaker.Code.Gameplay.Controllers;

public class FogOfWarRevealerTriggerController : UpdateController<FogOfWarRevealerTrigger>
{
	public FogOfWarRevealerTriggerController()
		: base(TickType.Simulation)
	{
	}
}
