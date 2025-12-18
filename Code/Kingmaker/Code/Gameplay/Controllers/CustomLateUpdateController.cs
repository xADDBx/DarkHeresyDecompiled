using Kingmaker.Controllers;
using Kingmaker.Controllers.Interfaces;

namespace Kingmaker.Code.Gameplay.Controllers;

public class CustomLateUpdateController : UpdateController<IUpdatable>
{
	public CustomLateUpdateController()
		: base(TickType.EndOfFrame)
	{
	}
}
