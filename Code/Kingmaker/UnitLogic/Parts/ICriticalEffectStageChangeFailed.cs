using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UnitLogic.Parts;

public interface ICriticalEffectStageChangeFailed : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleCriticalEffectStageChangeFailed(BlueprintBodyPart bodyPart, int failedStage);
}
