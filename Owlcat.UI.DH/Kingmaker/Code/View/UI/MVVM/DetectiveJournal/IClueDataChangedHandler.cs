using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public interface IClueDataChangedHandler : ISubscriber
{
	void RefreshDataFor(BlueprintClue clue);
}
