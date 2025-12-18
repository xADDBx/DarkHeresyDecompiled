using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ICombatEndHandler : ISubscriber
{
	void HandleCombatEnd(CombatEndReason reason);
}
