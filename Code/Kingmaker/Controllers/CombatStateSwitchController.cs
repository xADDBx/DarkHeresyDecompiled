using Kingmaker.Code.View.Bridge.Facades;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Controllers;

public class CombatStateSwitchController : IController, IPartyCombatHandler, ISubscriber
{
	public void HandlePartyCombatStateChanged(bool inCombat)
	{
		if (inCombat && !Game.Instance.Controllers.SelectionCharacter.IsSingleSelected.Value && Game.Instance.Controllers.SelectionCharacter.FirstSelectedUnit != null)
		{
			SelectionManagerFacade.SelectUnit(Game.Instance.Controllers.SelectionCharacter.FirstSelectedUnit.View);
		}
	}
}
