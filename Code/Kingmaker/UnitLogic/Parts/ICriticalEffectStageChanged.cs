using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UnitLogic.Parts;

public interface ICriticalEffectStageChanged : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleCriticalEffectStageChanged(BlueprintBodyPart bodyPart, int previous, int current);
}
public interface ICriticalEffectStageChanged<TTag> : ICriticalEffectStageChanged, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<ICriticalEffectStageChanged, TTag>
{
}
