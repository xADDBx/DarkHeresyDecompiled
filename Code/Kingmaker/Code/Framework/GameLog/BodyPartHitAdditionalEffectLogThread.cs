using System;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.Log.GameLogCntxt;

namespace Kingmaker.Code.Framework.GameLog;

public class BodyPartHitAdditionalEffectLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventBodyPartHitAdditionalEffect>
{
	public void HandleEvent(GameLogEventBodyPartHitAdditionalEffect evt)
	{
		BlueprintBodyPart bodyPart = evt.BodyPart;
		MechanicEntity entity = evt.Entity;
		GameLogContext.SourceEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)entity;
		GameLogContext.Description = bodyPart.Name.Text;
		GameLogContext.Text = evt.AdditionalText;
		switch (evt.Type)
		{
		case BodyPartHitAdditionalEffectType.BreakConcentration:
			AddMessage(LogThreadBase.Strings.BodyPartHitBreakConcentration.CreateCombatLogMessage(null, null, isPerformAttackMessage: false, entity));
			break;
		case BodyPartHitAdditionalEffectType.ChangeTurn:
			AddMessage(LogThreadBase.Strings.BodyPartHitChangeTurn.CreateCombatLogMessage(null, null, isPerformAttackMessage: false, entity));
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}
}
