using Kingmaker.Controllers;
using Kingmaker.Controllers.Interfaces;

namespace Kingmaker.Code.Gameplay.Controllers;

public class CustomUpdateController : UpdateController<IUpdatable>
{
	public CustomUpdateController()
		: base(TickType.Simulation)
	{
	}
}
