using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM.View;

public interface IRankEntryConfirmClickHandler : ISubscriber
{
	void OnRankEntryConfirmClick();
}
