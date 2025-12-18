using Kingmaker.Code.View.UI.MVVM.DetectiveJournal;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.Events;

public interface INewInfoHandler : ISubscriber
{
	void HandleMarkAsViewed(InfoWrapper info);
}
