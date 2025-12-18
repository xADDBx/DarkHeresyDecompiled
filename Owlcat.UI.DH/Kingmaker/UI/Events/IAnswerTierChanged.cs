using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.Events;

public interface IAnswerTierChanged : ISubscriber
{
	void HandleAnswerTierChanged(BlueprintCaseAnswer answer, int oldTier, int newTier);
}
