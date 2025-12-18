using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs;

namespace Kingmaker.UnitLogic.Parts;

public interface IBodyPartHitAdditionalEffect : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleBodyPartHitBreakConcentration(BlueprintBodyPart bodyPart, Buff concentrationBuff);

	void HandleBodyPartHitChangeTurn(BlueprintBodyPart bodyPart);
}
public interface IBodyPartHitAdditionalEffect<TTag> : IBodyPartHitAdditionalEffect, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IBodyPartHitAdditionalEffect, TTag>
{
}
