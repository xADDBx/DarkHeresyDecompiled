using JetBrains.Annotations;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IDetectiveJournalUIHandler : ISubscriber
{
	void HandleOpenDetectiveJournal([CanBeNull] BlueprintCase caseToOpen, BlueprintClue focusClue = null, bool requireReport = false);

	void HandleUnknownClues();
}
