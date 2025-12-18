using System.Linq;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.QA;

namespace Kingmaker.Controllers.Combat;

public class GameOverController : IControllerTick, IController
{
	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		if (!Megatron.IsActive)
		{
			Player.GameOverReasonType? gameOverReasonType = null;
			if (Game.Instance.Player.PartyAndPets.All((BaseUnitEntity u) => !u.LifeState.IsConscious))
			{
				gameOverReasonType = Player.GameOverReasonType.PartyIsDefeated;
			}
			else if (Game.Instance.Player.GameOverReason.HasValue)
			{
				gameOverReasonType = Game.Instance.Player.GameOverReason.Value;
			}
			if (gameOverReasonType.HasValue)
			{
				Game.Instance.Player.GameOver(gameOverReasonType.Value);
			}
		}
	}
}
