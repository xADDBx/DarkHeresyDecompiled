using Kingmaker.AreaLogic.Cutscenes.Commands.Timeline;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Interfaces;

namespace Kingmaker.Code.Gameplay.Controllers;

public class DirectorAdapterController : UpdateController<DirectorAdapter>
{
	public DirectorAdapterController()
		: base(TickType.Simulation)
	{
	}
}
