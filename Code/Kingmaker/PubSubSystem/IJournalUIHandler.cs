using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IJournalUIHandler : ISubscriber
{
	void HandleOpenJournal(Quest questToOpen = null);
}
