using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ISaveLoadUIHandler : ISubscriber
{
	void HandleOpenSaveLoad(SaveLoadMode mode, bool singleMode);
}
