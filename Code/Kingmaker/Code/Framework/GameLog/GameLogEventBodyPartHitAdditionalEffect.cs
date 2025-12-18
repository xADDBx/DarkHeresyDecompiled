using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.Code.Framework.GameLog;

public class GameLogEventBodyPartHitAdditionalEffect : GameLogEvent<GameLogEventBodyPartHitAdditionalEffect>
{
	private class EventsHandler : GameLogController.GameEventsHandler, IBodyPartHitAdditionalEffect, ISubscriber<IMechanicEntity>, ISubscriber
	{
		public void HandleBodyPartHitBreakConcentration(BlueprintBodyPart bodyPart, Buff concentrationBuff)
		{
			AddEvent(new GameLogEventBodyPartHitAdditionalEffect(EventInvokerExtensions.MechanicEntity, bodyPart, BodyPartHitAdditionalEffectType.BreakConcentration, concentrationBuff.Name));
		}

		public void HandleBodyPartHitChangeTurn(BlueprintBodyPart bodyPart)
		{
			AddEvent(new GameLogEventBodyPartHitAdditionalEffect(EventInvokerExtensions.MechanicEntity, bodyPart, BodyPartHitAdditionalEffectType.ChangeTurn, null));
		}
	}

	public readonly BodyPartHitAdditionalEffectType Type;

	public readonly MechanicEntity Entity;

	public readonly BlueprintBodyPart BodyPart;

	public readonly string AdditionalText;

	public GameLogEventBodyPartHitAdditionalEffect(MechanicEntity entity, BlueprintBodyPart bodyPart, BodyPartHitAdditionalEffectType type, string additionalText)
	{
		Type = type;
		Entity = entity;
		BodyPart = bodyPart;
		AdditionalText = additionalText;
	}
}
