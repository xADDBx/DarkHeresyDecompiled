using Kingmaker.Networking.NetGameFsm;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface INetEvents : ISubscriber
{
	void HandleTransferProgressChanged(bool value);

	void HandleNetGameStateChanged(NetGame.State state);

	void HandleNLoadingScreenClosed();
}
