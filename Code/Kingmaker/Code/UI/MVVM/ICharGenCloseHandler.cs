using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM;

public interface ICharGenCloseHandler : ISubscriber
{
	void HandleClose(bool withComplete, bool syncPortrait);
}
