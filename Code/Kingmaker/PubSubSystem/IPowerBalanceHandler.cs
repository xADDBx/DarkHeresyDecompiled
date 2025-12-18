using Kingmaker.Code.Gameplay.Controllers;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IPowerBalanceHandler : ISubscriber
{
	void HandlePowerBalanceValueUpdate(MoraleGroup combatGroup);

	void HandlePowerBalanceStateUpdate(MoraleGroup combatGroup, PowerBalanceState state);

	void HandlePowerBalanceRecalculated();
}
