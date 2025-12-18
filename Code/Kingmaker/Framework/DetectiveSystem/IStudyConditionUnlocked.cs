using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Framework.DetectiveSystem;

public interface IStudyConditionUnlocked : ISubscriber
{
	void HandleStudyUnlockedCondition(BlueprintClueStudy study);
}
