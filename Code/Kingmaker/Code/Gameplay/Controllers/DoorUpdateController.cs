using Kingmaker.Controllers;
using Kingmaker.Controllers.Interfaces;

namespace Kingmaker.Code.Gameplay.Controllers;

public class DoorUpdateController : UpdateController<IUpdatable>
{
	public DoorUpdateController()
		: base(TickType.Simulation)
	{
	}
}
