using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ICharGenInitiateUIHandler : ISubscriber
{
	void HandleStartCharGen(CharGenConfig config, bool isCustomCompanionChargen);
}
