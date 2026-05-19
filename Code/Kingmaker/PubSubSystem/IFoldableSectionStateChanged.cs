using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IFoldableSectionStateChanged : ISubscriber
{
	void HandleFoldableSectionStateChanged(string sectionKey, bool isExpanded);
}
