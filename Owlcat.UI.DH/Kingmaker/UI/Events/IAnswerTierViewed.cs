using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.Events;

public interface IAnswerTierViewed : ISubscriber
{
	void HandleAnswerTierViewed(BlueprintCaseAnswer answer);
}
