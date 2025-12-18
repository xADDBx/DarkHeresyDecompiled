using Kingmaker.Controllers;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Visual.CharactersRigidbody;

namespace Kingmaker.Code.Gameplay.Controllers;

public class UpdateRigidbodyCreatureController : UpdateController<RigidbodyCreatureController>
{
	public UpdateRigidbodyCreatureController()
		: base(TickType.Simulation)
	{
	}
}
