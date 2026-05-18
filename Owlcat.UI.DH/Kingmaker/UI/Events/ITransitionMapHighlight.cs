using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.Events;

public interface ITransitionMapHighlight : ISubscriber
{
	void HandleHighlightEntry(string areaName, bool highlight);
}
