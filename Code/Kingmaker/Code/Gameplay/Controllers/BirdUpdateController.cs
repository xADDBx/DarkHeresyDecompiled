using Kingmaker.Controllers;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Visual.Critters;

namespace Kingmaker.Code.Gameplay.Controllers;

public class BirdUpdateController : UpdateController<Bird>
{
	public BirdUpdateController()
		: base(TickType.Simulation)
	{
	}
}
