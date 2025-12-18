using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public interface IConclusionsUpdateHandler : ISubscriber
{
	void UpdateConclusions();
}
