using Kingmaker.Controllers;
using Kingmaker.Controllers.Interfaces;

namespace Kingmaker.Code.Gameplay.Controllers;

public class CustomUpdateBeforePhysicsController : UpdateController<IUpdatable>
{
	public CustomUpdateBeforePhysicsController()
		: base(TickType.Simulation)
	{
	}
}
