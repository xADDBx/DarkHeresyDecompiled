using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM;

public interface IRankEntryFocusHandler : ISubscriber
{
	void SetFocusOn(BaseRankEntryFeatureVM featureVM);
}
