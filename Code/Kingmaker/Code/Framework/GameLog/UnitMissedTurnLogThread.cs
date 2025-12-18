using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.View.Bridge.Services;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Enums;
using Owlcat.UI;

namespace Kingmaker.Code.Framework.GameLog;

public class UnitMissedTurnLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventUnitMissedTurn>
{
	private static readonly MechanicsFeatureType[] Conditions = new MechanicsFeatureType[3]
	{
		MechanicsFeatureType.Stunned,
		MechanicsFeatureType.Prone,
		MechanicsFeatureType.Sleeping
	};

	public void HandleEvent(GameLogEventUnitMissedTurn evt)
	{
		if (!(evt.Actor.Entity is BaseUnitEntity baseUnitEntity))
		{
			return;
		}
		List<FeatureCountableFlag.FactsList.Element> list = baseUnitEntity.GetMechanicFeature(MechanicsFeatureType.CantAct).AssociatedFacts.Elements.ToList();
		MechanicsFeatureType[] conditions = Conditions;
		foreach (MechanicsFeatureType type in conditions)
		{
			foreach (EntityFact fact2 in baseUnitEntity.GetMechanicFeature(type).AssociatedFacts.Facts)
			{
				if (fact2 is Buff fact)
				{
					list.Add(new FeatureCountableFlag.FactsList.Element(fact));
				}
			}
		}
		GameLogContext.TargetEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)baseUnitEntity;
		CombatLogMessage combatLogMessage = LogThreadBase.Strings.UnitMissedTurn.CreateCombatLogMessage();
		if (list.Count > 0)
		{
			TooltipBaseTemplate tooltipBaseTemplate = combatLogMessage?.Tooltip;
			if (tooltipBaseTemplate != null)
			{
				CombatLogTooltipService.SetTooltipTemplateCombatLogMessageExtraBricks(tooltipBaseTemplate, CollectExtraBricks(list).ToArray(), arg3: false);
				CombatLogTooltipService.SetTooltipTemplateCombatLogMessageExtraBricks(tooltipBaseTemplate, CollectExtraBricks(list).ToArray(), arg3: true);
			}
		}
		AddMessage(combatLogMessage);
	}

	private IEnumerable<ITooltipBrick> CollectExtraBricks(List<FeatureCountableFlag.FactsList.Element> buffs)
	{
		Func<TooltipBrickIconTextValueArgs, ITooltipBrick> createTooltipBrickIconTextValue = CombatLogTooltipService.CreateTooltipBrickIconTextValue;
		Func<TooltipBrickTextValueArgs, ITooltipBrick> textTemplate = CombatLogTooltipService.CreateTooltipBrickTextValue;
		if (createTooltipBrickIconTextValue == null || textTemplate == null)
		{
			yield break;
		}
		yield return createTooltipBrickIconTextValue(new TooltipBrickIconTextValueArgs(LogThreadBase.Strings.TooltipBrickStrings.Reasons.Text, "", 1, isResultValue: false, null, isProtectionIcon: false, isTargetHitIcon: false, isBorderChanceIcon: false, isGrayBackground: true));
		foreach (FeatureCountableFlag.FactsList.Element buff in buffs)
		{
			yield return textTemplate(new TooltipBrickTextValueArgs(buff.BuffInformation.Name, "", 1));
		}
	}
}
